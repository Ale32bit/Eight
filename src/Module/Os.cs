using System;
using System.Collections.Generic;
using KeraLua;

namespace Eight.Module {
    public static class Os {
        public static string[] Whitelist = {
            "time",
            "difftime",
            "clock",
            "date"
        };

        public static List<LuaRegister> OSLib = new();

        public static void Setup() {
            var LuaState = Runtime.LuaState;

            OSLib.Add(new() {
                name = "version",
                function = Version
            });

            OSLib.Add(new() {
                name = "exit",
                function = Exit
            });

            OSLib.Add(new() {
                name = "reboot",
                function = Reboot,
            });

            OSLib.Add(new() {
                name = "flag",
                function = Flag,
            });

            OSLib.Add(new() {
                name = "setRPC",
                function = SetRPC,
            });

            OSLib.Add(new() {
                name = "getRPC",
                function = GetRpc,
            });

            foreach ( var name in Whitelist ) {
                LuaState.GetGlobal("os");
                var funcType = LuaState.GetField(-1, name);
                if ( funcType != LuaType.Function ) continue;
                var func = LuaState.ToCFunction(-1);
                OSLib.Add(new LuaRegister {
                    name = name,
                    function = func
                });
            }

            LuaState.PushNil();
            LuaState.SetGlobal("os");

            LuaState.CreateTable(0, OSLib.Count);

            foreach ( var reg in OSLib ) {
                LuaState.PushCFunction(reg.function);
                LuaState.SetField(-2, reg.name);
            }

            LuaState.SetGlobal("os");
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
                if(Eight.Flags.TryGetValue(flagName, out bool value)) {
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

            if ( !state.IsNoneOrNil(2)) {
               rpcState =  state.CheckString(2);
            }

            if(Eight.Flags["allow_rpc_change"]) 
                Discord.SetStatus(rpcDetails, rpcState);

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