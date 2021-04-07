using KeraLua;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eight.Module {
    class Thread {
        public static LuaRegister[] ThreadLib = {
            new() {
                function = Launch,
                name = "launch"
            },
            new()
        };

        private static List<Lua> Threads = new();

        public static void Setup() {
            Runtime.LuaState.RequireF("thread", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(ThreadLib);
            return 1;
        }

        private static int Launch(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            Console.WriteLine("thread.launch is unstable, use with caution.", true);

            string script = state.CheckString(1);

            var thread = Runtime.State.NewThread();

            Threads.Add(thread);
            var id = Threads.Count - 1;

            var status = thread.LoadString(script, "@Thread<" + id + ">");

            if ( status != LuaStatus.OK ) {
                var error = thread.ToString(-1);
                state.PushBoolean(false);
                state.PushString(error);
                return 2;
            }

            Run(id);

            state.PushBoolean(true);
            state.PushNumber(id);

            return 2;
        }

        private static void Run(int id) {
            Lua state = Threads[id];
            Task task = new Task(() => {
                Event.Push(new Utils.LuaParameter[] {
                    new() {
                        Value = "thread_start",
                        Type = LuaType.String,
                    },
                    new() {
                        Value = id,
                        Type = LuaType.Number,
                    },
                });
                while ( true ) {
                    var status = state.Resume(null, 0, out var nres);
                    if ( status == LuaStatus.OK || status == LuaStatus.Yield ) {
                        Runtime.State.Pop(nres);
                        if ( status != LuaStatus.OK ) continue;
                        Event.Push(new Utils.LuaParameter[] {
                            new() {
                                Value = "thread_finish",
                                Type = LuaType.String,
                            },
                            new() {
                                Value = id,
                                Type = LuaType.Number,
                            },
                        });
                        Threads.RemoveAt(id);
                        break;
                    }

                    var error = state.ToString(-1) ?? "Unknown Error";

                    Event.Push(new Utils.LuaParameter[] {
                        new() {
                            Value = "thread_fail",
                            Type = LuaType.String,
                        },
                        new() {
                            Value = id,
                            Type = LuaType.Number,
                        },
                        new() {
                            Value = error,
                            Type = LuaType.String,
                        },
                    });
                    Threads.RemoveAt(id);
                    break;
                }
            });

            task.Start();
        }
    }
}
