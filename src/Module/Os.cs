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

            foreach (var name in Whitelist) {
                LuaState.GetGlobal("os");
                var funcType = LuaState.GetField(-1, name);
                if (funcType != LuaType.Function) continue;
                var func = LuaState.ToCFunction(-1);
                OSLib.Add(new LuaRegister {
                    name = name,
                    function = func
                });
            }

            LuaState.PushNil();
            LuaState.SetGlobal("os");

            LuaState.CreateTable(0, OSLib.Count);

            foreach (var reg in OSLib) {
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

        public static int Exit(IntPtr state) {
            Eight.Quit();
            return 0;
        }

        public static int Reboot(IntPtr state) {
            Eight.Reset();
            return 0;
        }
    }
}