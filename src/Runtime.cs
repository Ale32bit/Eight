using Eight.Module;
using KeraLua;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Eight {
    public static class Runtime {
        public static Lua LuaState;
        public static Lua State;

        private static bool _running;
        private static bool _quit;
        private static bool _killed;

        private static string[] crashSplashes = {
            "Why are we here?",
            "Here! Have a cookie!",
            "The crash has crashed.",
            "Press F to pay respects",
            "Crap! Not again!",
            "Why am I wasting my time writing these?!",
            "Give me some coffee, please!",
            "<3 SquidDev",
            "Eight.Quit();",
            "Now crash this."
        };
        public static bool Init() {
            _quit = false;
            LuaState = new Lua {
                Encoding = Encoding.UTF8
            };

            LuaState.PushString($"Eight {Eight.Version}");
            LuaState.SetGlobal("_HOST");

            LuaState.SetWarningFunction(WarnFunction, IntPtr.Zero);

            DoLibs(LuaState, true);

            State = LuaState.NewThread();

            State.SetHook((luaState, ar) => {
                var state = Lua.FromIntPtr(luaState);

                var arg = LuaDebug.FromIntPtr(ar);

                if ( arg.Event == LuaHookEvent.Count ) {
                    if ( Event.OutOfSync && !_killed ) {
                        _killed = true;
                        if ( Eight.Flags["out_of_sync_error"] )
                            State.Error("out of sync");
                    }
                }
            }, LuaHookMask.Count, 10000);

            if ( !File.Exists("Lua/firmware.lua") ) {
                Console.WriteLine("Could not find firmware.lua");
                Eight.Crash("Could not find firmware.lua!", "Is Eight installed correctly?");
                return false;
            }

            var bootContent = File.ReadAllText("Lua/boot.lua");

            var btStatus = State.LoadString(bootContent, "@BOOT");
            if ( btStatus != LuaStatus.OK ) {
                var error = State.ToString(-1);
                Console.WriteLine("Lua Load Exception: {0}", error);
                return false;
            }

            return true;
        }

        private static void DoLibs(Lua state, bool completeLoad) {
            // Get io.open and io.lines for filesystem
            state.GetGlobal("io");
            state.GetField(-1, "open");
            state.SetField((int)LuaRegistry.Index, "_io_open");
            state.Pop(1);

            state.GetGlobal("io");
            state.GetField(-1, "lines");
            state.SetField((int)LuaRegistry.Index, "_io_lines");
            state.Pop(1);

            // Get debug.traceback
            state.GetGlobal("debug");
            state.GetField(-1, "traceback");
            state.SetField((int)LuaRegistry.Index, "_debug_traceback");
            state.Pop(1);

            // Destroy dem libtards with shapiro

            state.PushNil();
            state.SetGlobal("debug");

            state.PushNil();
            state.SetGlobal("io");

            state.PushNil();
            state.SetGlobal("dofile");

            state.PushNil();
            state.SetGlobal("loadfile");

            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(IModule))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as IModule;

            foreach ( var instance in instances ) {
                if(completeLoad || (!completeLoad && instance.ThreadReady))
                    instance.Init(state);
            }

            var fwContent = File.ReadAllText("Lua/firmware.lua");

            var fwStatus = state.LoadString(fwContent, "@FIRMWARE");
            if ( fwStatus != LuaStatus.OK ) {
                var error = State.ToString(-1);
                Console.WriteLine("Lua Load Exception: {0}", error);
            }
        }

        public static bool Resume(int n = 0) {
            if ( _quit ) return false;
            if ( _running ) return false;
            _running = true;
            var status = State.Resume(null, n, out var nres);
            _running = false;
            _killed = false;
            if ( status == LuaStatus.OK || status == LuaStatus.Yield ) {
                State.Pop(nres);
                if ( status != LuaStatus.OK ) return true;
                Console.WriteLine(State.ToString(-1));
                return false;
            }

            var error = State.ToString(-1) ?? "Unknown Error";
            State.Traceback(State);
            var traceback = State.ToString(-1) ?? "Unknown Trace";

            string nr;
            switch ( status ) {
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

            var hexStatus = status.ToString("X").TrimStart('0');
            Console.WriteLine($"Lua Exception [0x{hexStatus}] {nr}: {error}");
            Console.WriteLine(traceback);
            Console.WriteLine("Could not resume");

            var rand = new Random();
            var splash = crashSplashes[rand.Next(crashSplashes.Length)];

            Eight.Crash(splash, error, traceback, "Could not resume");

            return false;
        }

        public static unsafe void WarnFunction(IntPtr ud, IntPtr msg, int tocont) {
            var message = Marshal.PtrToStringAnsi(msg);
            Console.WriteLine($"[WARN] {message}");
        }

        public static void Quit() {
            _quit = true;

            if ( State != null ) State.Close();
        }
    }
}