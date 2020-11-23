using System;
using System.Net.Http;
using KeraLua;

namespace Eight.LuaLibs {
    public static class HTTP {
        
        
        public static LuaRegister[] HTTP_Lib = {
            new LuaRegister {
                function = Request,
                name = "request"
            }, 
            new LuaRegister(), 
        };

        public static void Setup() {
            Logic.Lua.LuaState.RequireF("http", Open, false);
        }

        public static int Open(IntPtr luaState) {
            
            return 0;
        }

        public static int Request(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            var url = state.ToString(1);
            HTTPHandler.Request(url);
            return 0;
        }
    }
}