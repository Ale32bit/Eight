using KeraLua;
using System;
using System.Collections.Generic;

namespace Eight.Module {
    public class Os : IModule {
        public bool ThreadReady {
            get => true;
        }

        public static string[] Whitelist = {
            "time",
            "difftime",
            "clock",
            "date"
        };

        public static List<LuaRegister> OSLib = new(new LuaRegister[] {
            new() {
                name = "version",
                function = Version
            },
            new() {
                name = "exit",
                function = Exit
            },
            new() {
                name = "reboot",
                function = Reboot,
            },
            new() {
                name = "flag",
                function = Flag,
            },
            new() {
                name = "setRPC",
                function = SetRPC,
            },
            new() {
                name = "getRPC",
                function = GetRpc,
            },
        });

        public void Init(Lua state) {

            foreach ( var name in Whitelist ) {
                state.GetGlobal("os");
                var funcType = state.GetField(-1, name);
                if ( funcType != LuaType.Function ) {
                    continue;
                }

                var func = state.ToCFunction(-1);
                OSLib.Add(new LuaRegister {
                    name = name,
                    function = func
                });
            }

            state.PushNil();
            state.SetGlobal("os");

            state.CreateTable(0, OSLib.Count);

            foreach ( var reg in OSLib ) {
                state.PushCFunction(reg.function);
                state.SetField(-2, reg.name);
            }

            state.SetGlobal("os");
        }

        public static int Version(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushString(Eight.Version);

            return 1;
        }

        public static int Exit(IntPtr luaState) {
            Eight.Quit();
            return 0;
        }

        public static int Reboot(IntPtr luaState) {
            Eight.Reset();
            return 0;
        }

        public static int Flag(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckString(1);

            string flagName = state.ToString(1);

            if ( state.IsNoneOrNil(2) ) {
                if ( Eight.Flags.TryGetValue(flagName, out bool value) ) {
                    state.PushBoolean(value);
                } else {
                    state.PushBoolean(false);
                }

                return 1;
            } else {
                state.ArgumentCheck(state.IsBoolean(2), 2, "boolean expected");

                bool flagValue = state.ToBoolean(2);

                Eight.Flags[flagName] = flagValue;
            }

            return 0;
        }

        public static int SetRPC(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string rpcDetails = state.CheckString(1);
            string rpcState = "";

            if ( !state.IsNoneOrNil(2) ) {
                rpcState = state.CheckString(2);
            }

            if ( Eight.Flags["allow_rpc_change"] ) {
                Discord.SetStatus(rpcDetails, rpcState);
            }

            return 0;
        }

        public static int GetRpc(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushString(Discord.Client.CurrentPresence.Details);
            state.PushString(Discord.Client.CurrentPresence.State);

            return 2;
        }
    }
}