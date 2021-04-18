using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Eight.Module {
    class TCP : IModule {
        public static LuaRegister[] TCPLib = {
            new() {
                function = Open,
                name = "open"
            },
            new()
        };

        public bool ThreadReady => true;

        public void Init(Lua state) {
            state.RequireF("tcp", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(TCPLib);
            return 1;
        }

        private static int Open(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            if ( !BIOS.biosConfig.EnableInternet ) {
                state.PushBoolean(false);
                state.PushString("Internet is disabled from BIOS");
                return 2;
            }

            string server = state.CheckString(1);
            int port = (int)state.CheckNumber(2);


            

            return 0;
        }

    }
}
