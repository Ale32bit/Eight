using System;
using System.Collections.Generic;
using System.Timers;
using KeraLua;
using SDL2;

namespace Eight.LuaLibs {
    public static class Timer {
        private static Dictionary<int, System.Timers.Timer> _timers = new Dictionary<int, System.Timers.Timer>();

        private static LuaRegister[] timer_lib = {
            new LuaRegister {
                function = Start,
                name = "start",
            },
            new LuaRegister() {
                function = Stop,
                name = "stop",
            },
            new LuaRegister(), // Null
        };

        public static void Setup() {
            Logic.Lua.LuaState.RequireF("timer", Open, false);
        }

        private static int Open(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(timer_lib);
            return 1;
        }

        public static int Start(IntPtr s) {
            var State = Lua.FromIntPtr(s);

            int ms = (int) State.ToInteger(1);

            State.PushInteger(StartTimer(ms));
            return 1;
        }

        public static int Stop(IntPtr s) {
            var State = Lua.FromIntPtr(s);

            int timerId = (int) State.ToInteger(1);

            State.PushBoolean(StopTimer(timerId));
            return 1;
        }

        public static int StartTimer(int ms) {
            var timer = new System.Timers.Timer {
                Interval = ms,
                AutoReset = false,
                Enabled = true
            };

            int timerId = (DateTime.Now - Eight.Epoch).GetHashCode();

            timer.Elapsed += (sender, e) => TimerHandler(sender, e, timerId);
            timer.Start();

            _timers.Add(timerId, timer);
            return timerId;
        }

        public static bool StopTimer(int timerId) {
            if (!_timers.ContainsKey(timerId)) return false;

            var timer = _timers[timerId];
            timer.Enabled = false;
            timer.Stop();

            _timers.Remove(timerId);

            return true;
        }

        private static void TimerHandler(object sender, ElapsedEventArgs e, int timerId) {
            if (!_timers.ContainsKey(timerId)) return;

            var ev = new SDL.SDL_Event {type = SDL.SDL_EventType.SDL_USEREVENT, user = {code = 1}};
            ev.user.data1 = (IntPtr) timerId;
            SDL.SDL_PushEvent(ref ev);

            _timers.Remove(timerId);
        }
    }
}