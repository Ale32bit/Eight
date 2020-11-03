using System;
using System.Reflection;
using System.Reflection.Emit;
using Eight.LuaLibs;
using State = KeraLua;
using NLua;

namespace Eight {
    public class LuaLogic {
        public static Lua L;
        public static State.Lua sL;

        public static bool Init() {
            L = new Lua();

            L["_HOST"] = "Eight " + Eight.Version;
            
            Sandbox();

            AddLibs();
            
            sL = L.State.NewThread();


            State.LuaStatus status = sL.LoadFile("init.lua");

            if (status > 0) {
                string error = sL.ToString(-1);
                Console.WriteLine("Lua State exception: {0}", error);
                return false;
            }


            return true;
        }

        public static bool Resume(int n) {
            var ok = sL.Resume(L.State, n, out var nres);

            if (ok == State.LuaStatus.OK || ok == State.LuaStatus.Yield) {
                sL.Pop(nres);
                if (ok == State.LuaStatus.OK) {
                    Console.WriteLine(sL.ToString(-1));
                    return false;
                }
            }
            else {
                string error = sL.ToString(-1) ?? "Unknown Error";

                Console.WriteLine("Lua Exception: {0}", error);

                string nr;
                switch (nres) {
                    case (int) State.LuaStatus.OK:
                        nr = "OK";
                        break;
                    case (int) State.LuaStatus.ErrRun:
                        nr = "ERRRUN";
                        break;
                    case (int) State.LuaStatus.ErrMem:
                        nr = "ERRMEM";
                        break;
                    case (int) State.LuaStatus.ErrErr:
                        nr = "ERRERR";
                        break;
                    case (int) State.LuaStatus.ErrSyntax:
                        nr = "ERRSYNTAX";
                        break;
                    case (int) State.LuaStatus.Yield:
                        nr = "YIELD";
                        break;
                    default:
                        nr = nres.ToString();
                        break;
                }
                
                Console.WriteLine("NRESULT [{0:X}] {1}", nres, nr);

                return false;
            }

            return true;
        }

        private static void Sandbox() {
            L["os"] = null;
            L["io"] = null;
        }

        private static void AddLibs() {
            Screen screen = new Screen();
            L["screen"] = screen;
        }
    }
}