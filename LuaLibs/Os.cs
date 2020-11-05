using System.Threading;
using System.Timers;
using NLua.Exceptions;

namespace Eight.LuaLibs {
    public class Os {
        public Os() {
            
        }

        public int Timer(int ms) {
            return LuaLogic.SetTimer(ms);
        }

        public string Version() {
            return Eight.Version;
        }

        public void Quit() {
            Eight.Quit();
        }

        public void Reset() {
            // Disabled because not working
            //Eight.Reset();
        }
    }
}