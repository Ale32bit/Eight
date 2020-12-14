using System;
using System.Collections.Generic;
using System.Timers;
using KeraLua;
using SDL2;
using Lua = Eight.Logic.Lua;

namespace Eight.LuaLibs {
    public static class Timer {
        private static readonly Dictionary<int, System.Timers.Timer> _timers = new();

        private static readonly LuaRegister[] timer_lib = {
            new() {
                function = Start,
                name = "start"
            },
            new() {
                function = Stop,
                name = "stop"
            },
            new() // Null
        };

        public static void Setup() {
            Lua.LuaState.RequireF("timer", Open, false);
        }

        private static int Open(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.NewLib(timer_lib);
            return 1;
        }

        public static int Start(IntPtr s) {
            var state = KeraLua.Lua.FromIntPtr(s);
            
            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");

            var ms = (int) state.ToInteger(1);

            state.PushInteger(StartTimer(ms));
            return 1;
        }

        public static int Stop(IntPtr s) {
            var state = KeraLua.Lua.FromIntPtr(s);
            
            state.ArgumentCheck(state.IsInteger(1), 1, "expected integer");

            var timerId = (int) state.ToInteger(1);

            state.PushBoolean(StopTimer(timerId));
            return 1;
        }

        public static int StartTimer(int ms) {
            var timer = new System.Timers.Timer {
                Interval = ms,
                AutoReset = false,
                Enabled = true
            };

            var timerId = (DateTime.Now - Eight.Epoch).GetHashCode();

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
            
            Utils.LuaParameter[] parameters = {
                new() {
                    Type = LuaType.String,
                    Value = "timer"
                },
                new() {
                    Type = LuaType.Number,
                    Value = (double) timerId,
                }
            };

            Eight.PushEvent(parameters);

            _timers.Remove(timerId);
        }
    }
}