using System;
using System.Text;
using KeraLua;

namespace Eight.Logic {
    public static class Lua {
        public static KeraLua.Lua LuaState;
        public static KeraLua.Lua State;

        private static bool _quit;

        public static bool Init() {
            LuaState = new KeraLua.Lua {
                Encoding = Encoding.UTF8
            };

            //LuaState.SetHook(Hook, LuaHookMask.Count, 100000);
            
            LuaState.PushString($"Eight {Eight.Version}");
            LuaState.SetGlobal("_HOST");

            DoLibs();

            State = LuaState.NewThread();

            var status = State.LoadFile("bios.lua");
            if (status != LuaStatus.OK) {
                var error = State.ToString(-1);
                Console.WriteLine("Lua Load Exception: {0}", error);
                return false;
            }

            return true;
        }

        private static void DoLibs() {
            // Destroy dem libtards with shapiro
            LuaState.PushNil();
            LuaState.SetGlobal("io");

            LuaLibs.FileSystem.Setup();
            LuaLibs.Os.Setup();
            LuaLibs.Timer.Setup();
            LuaLibs.Screen.Setup();
        }

        private static void Hook(IntPtr luaState, IntPtr luaDebug) {
            KeraLua.Lua state = KeraLua.Lua.FromIntPtr(luaState);
            LuaDebug debug = LuaDebug.FromIntPtr(luaDebug);

            //Console.WriteLine("shit happened");
            //Console.WriteLine($"{debug.Name} {debug.Source}");
            State.Error("hi");
        }

        public static bool Resume(int n = 0) {
            if (_quit) return false;
            var status = State.Resume(null, n, out var nres);
            if (status == LuaStatus.OK || status == LuaStatus.Yield) {
                State.Pop(nres);
                if (status != LuaStatus.OK) return true;
                Console.WriteLine(State.ToString(-1));
                return false;
            }

            string error = State.ToString(-1) ?? "Unknown Error";
            State.Traceback(State);
            string traceback = State.ToString(-1) ?? "Unknown Trace";

            string nr;
            switch (status) {
                case LuaStatus.ErrRun:
                    nr = "Runtime Error";
                    break;
                case LuaStatus.ErrMem:
                    nr = "Memory Allocation Error";
                    break;
                case LuaStatus.ErrErr:
                    nr = "Error Handler Error";
                    break;
                case LuaStatus.ErrSyntax:
                    nr = "Syntax Error";
                    break;
                default:
                    nr = status.ToString();
                    break;
            }

            string hexStatus = status.ToString("X").TrimStart('0');
            Console.WriteLine($"Lua Exception [0x{hexStatus}] {nr}: {error}");
            Console.WriteLine(traceback);

            return false;
        }

        public static void Quit() {
            _quit = true;
            State.Close();
        }
    }
}