using System;
using System.Collections.Generic;
using KeraLua;

using System.Timers;

namespace Eight.Module {
    public static class Timer {
        private static readonly Dictionary<double, System.Timers.Timer> _timers = new();

        private static readonly LuaRegister[] TimerLib = {
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
            Runtime.LuaState.RequireF("timer", Open, false);
        }

        private static int Open(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(TimerLib);
            return 1;
        }

        public static int Start(IntPtr s) {
            var state = Lua.FromIntPtr(s);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");

            var ms = state.ToNumber(1);

            state.PushNumber(StartTimer(ms));
            return 1;
        }

        public static int Stop(IntPtr s) {
            var state = Lua.FromIntPtr(s);

            state.ArgumentCheck(state.IsInteger(1), 1, "expected integer");

            var timerId = state.ToNumber(1);

            state.PushBoolean(StopTimer(timerId));
            return 1;
        }

        public static double StartTimer(double ms) {
            var timer = new System.Timers.Timer {
                Interval = ms,
                AutoReset = false,
                Enabled = true
            };

            double timerId = (DateTime.Now - Eight.Epoch).TotalMilliseconds + ms;

            timer.Elapsed += (sender, e) => TimerHandler(sender, e, timerId);
            timer.Start();

            _timers.Add(timerId, timer);
            return timerId;
        }

        public static bool StopTimer(double timerId) {
            if (!_timers.ContainsKey(timerId)) return false;

            var timer = _timers[timerId];
            timer.Enabled = false;
            timer.Stop();

            _timers.Remove(timerId);

            return true;
        }

        private static void TimerHandler(object sender, ElapsedEventArgs e, double timerId) {
            if (!_timers.ContainsKey(timerId)) return;

            Utils.LuaParameter[] parameters = {
                new() {
                    Type = LuaType.String,
                    Value = "timer"
                },
                new() {
                    Type = LuaType.Number,
                    Value = timerId,
                }
            };

            Eight.PushEvent(parameters);

            _timers.Remove(timerId);
        }
    }
}