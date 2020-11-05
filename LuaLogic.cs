using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Eight.LuaLibs;
using State = KeraLua;
using NLua;
using NLua.Exceptions;
using static SDL2.SDL;

namespace Eight {
    public class LuaLogic {
        public static Lua L;
        public static State.Lua sL;
        public static bool IsResetting = false;
        
        private static Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();

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
            if (IsResetting) return true;
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
                switch (ok) {
                    case State.LuaStatus.ErrRun:
                        nr = "ERRRUN";
                        break;
                    case State.LuaStatus.ErrMem:
                        nr = "ERRMEM";
                        break;
                    case State.LuaStatus.ErrErr:
                        nr = "ERRERR";
                        break;
                    case State.LuaStatus.ErrSyntax:
                        nr = "ERRSYNTAX";
                        break;
                    default:
                        nr = ok.ToString();
                        break;
                }

                Console.WriteLine("LUA_STATUS [{0:X}] {1}", ok, nr);

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
            
            Os os = new Os();
            L["os"] = os;
        }

        public static int SetTimer(int interval) {
            var timer = new Timer {
                Interval = interval,
                AutoReset = false,
                Enabled = true
            };
            
            int timerId = (int) ((DateTime.Now - Eight.Epoch).GetHashCode());

            timer.Elapsed += (sender, e) => TimerHandler(sender, e, timerId);

            _timers.Add(timerId, timer);

            timer.Start();

            return timerId;
        }

        private static void TimerHandler(object sender, ElapsedEventArgs e, int timerId) {
            if (!_timers.ContainsKey(timerId)) return;
            
            var ev = new SDL_Event {type = SDL_EventType.SDL_USEREVENT, user = {code = 1}};
            ev.user.data1 = (IntPtr) timerId;
            SDL_PushEvent(ref ev);

            _timers.Remove(timerId);
        }

        // Annihilate the state
        public static void Reset() {
            // First destroy all timers
            foreach (var kv in _timers) {
                kv.Value.Start();
            }
            
            _timers.Clear();
            
            // Destroy the goddamn lua states
            L.State.Close();
        }
    }
}