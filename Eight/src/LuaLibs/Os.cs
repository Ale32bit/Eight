using System;
using System.Collections.Generic;
using State = KeraLua;
using Lua = KeraLua.Lua;

namespace Eight.LuaLibs {
    public static class Os {
        public static string[] Whitelist = {
            "time",
            "difftime",
            "clock"
        };


        public static List<State.LuaRegister> Os_lib = new List<State.LuaRegister>();

        public static void Setup() {
            var LuaState = Logic.Lua.LuaState;

            Os_lib.Add(new State.LuaRegister {
                name = "version",
                function = Version,
            });

            Os_lib.Add(new State.LuaRegister {
                name = "quit",
                function = Quit,
            });
            
            foreach (var name in Whitelist) {
                LuaState.GetGlobal("os");
                var funcType = LuaState.GetField(-1, name);
                if (funcType != State.LuaType.Function) continue;
                var func = LuaState.ToCFunction(-1);
                Os_lib.Add(new State.LuaRegister {
                    name = name,
                    function = func,
                });
            }
            
            LuaState.PushNil();
            LuaState.SetGlobal("os");

            LuaState.CreateTable(0, Os_lib.Count);

            foreach (var reg in Os_lib) {
                LuaState.PushCFunction(reg.function);
                LuaState.SetField(-2, reg.name);
            }
            
            LuaState.SetGlobal("os");
        }

        private static int Open(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(Os_lib.ToArray());
            return 1;
        }

        public static int Version(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushString(Eight.Version);

            return 1;
        }

        public static int Quit(IntPtr state) {
            Eight.Quit();
            return 0;
        }
    }
}
