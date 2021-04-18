using KeraLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eight.Module {
    class Thread : IModule {
        public bool ThreadReady {
            get => true;
        }

        public static LuaRegister[] ThreadLib = {
            new() {
                function = Launch,
                name = "launch"
            },
            new() {
                function = Push,
                name = "push",
            },
            new() {
                function = Pop,
                name = "pop",
            },
            new() {
                function = Peek,
                name = "peek",
            },
            new() {
                function = GetID,
                name = "getID",
            },
            new() {
                function = Sleep,
                name = "sleep",
            },
            new()
        };

        private static List<Lua> Threads = new();
        private static Dictionary<string, ConcurrentQueue<Utils.LuaParameter>> Channels = new();

        public void Init(Lua state) {
            state.PushString("master");
            state.SetField((int)LuaRegistry.Index, "_thread_id");
            state.Pop(1);
            state.RequireF("thread", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(ThreadLib);
            return 1;
        }

        private static int Launch(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            Console.WriteLine("thread.launch is unstable, use with caution.");

            string script = state.CheckString(1);

            var thread = new Lua(true);

            Threads.Add(thread);
            var id = Threads.Count - 1;

            thread.PushString(id.ToString());
            thread.SetField((int)LuaRegistry.Index, "_thread_id");
            thread.Pop(1);

            Runtime.DoLibs(thread, false);

            var status = thread.LoadString(script, "@Thread<" + id + ">");

            if ( status != LuaStatus.OK ) {
                Threads.RemoveAt(id);

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

        private static int GetID(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.GetField((int)LuaRegistry.Index, "_thread_id");

            return 1;
        }

        private static int Push(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string channel = state.CheckString(1);
            state.CheckAny(2);

            LuaType type;
            object value;

            if ( state.IsNil(2) ) {
                type = LuaType.Nil;
                value = null;
            } else if ( state.IsString(2) ) {
                type = LuaType.String;
                value = state.ToString(2);
            } else if ( state.IsNumber(2) ) {
                type = LuaType.Number;
                value = state.ToNumber(2);
            } else if ( state.IsBoolean(2) ) {
                type = LuaType.Boolean;
                value = state.ToBoolean(2);
            } else {
                state.ArgumentError(2, "expected nil, string, number or boolean");
                return 0;
            }

            ConcurrentQueue<Utils.LuaParameter> queue;

            if ( !Channels.TryGetValue(channel, out queue) ) {
                queue = new();
                Channels.Add(channel, queue);
            }

            queue.Enqueue(new() {
                Type = type,
                Value = value,
            });

            return 0;
        }

        private static int Pop(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string channel = state.CheckString(1);

            ConcurrentQueue<Utils.LuaParameter> queue;

            if ( !Channels.TryGetValue(channel, out queue) ) {
                return 0;
            }

            if ( !queue.TryDequeue(out var result) )
                return 0;

            switch ( result.Type ) {
                case LuaType.Nil:
                    state.PushNil();
                    break;
                case LuaType.String:
                    state.PushString((string)result.Value);
                    break;
                case LuaType.Number:
                    state.PushNumber((double)result.Value);
                    break;
                case LuaType.Boolean:
                    state.PushBoolean((bool)result.Value);
                    break;
            }

            return 1;
        }

        private static int Peek(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            string channel = state.CheckString(1);

            ConcurrentQueue<Utils.LuaParameter> queue;

            if ( !Channels.TryGetValue(channel, out queue) ) {
                return 0;
            }

            if ( !queue.TryPeek(out var result) )
                return 0;

            switch ( result.Type ) {
                case LuaType.Nil:
                    state.PushNil();
                    break;
                case LuaType.String:
                    state.PushString((string)result.Value);
                    break;
                case LuaType.Number:
                    state.PushNumber((double)result.Value);
                    break;
                case LuaType.Boolean:
                    state.PushBoolean((bool)result.Value);
                    break;
            }

            return 1;
        }

        private static int Sleep(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            System.Threading.Thread.Sleep((int)state.CheckNumber(1));

            return 0;
        }
    }
}
