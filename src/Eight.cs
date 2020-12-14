using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Eight.Logic;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;
using Lua = KeraLua;
using Timer = System.Timers.Timer;

namespace Eight {
    public static class Eight {
        public const string Version = "Alpha 0.0.5";

        public const int DefaultWidth = 200;
        public const int DefaultHeight = 150;
        public const int DefaultScale = 2;
        public const int DefaultTickrate = 60;

        public static readonly string BaseDir = Directory.GetCurrentDirectory();
        public static readonly string LuaDir = Path.Combine(BaseDir, "lua");

        public static int WindowWidth;
        public static int WindowHeight;
        public static int WindowScale;

        public static int Tickrate;
        public static int Ticktime;

        public static string[] Args;

        public static int SyncTimeout = 3000;
        public static bool OutOfSync;

        public static readonly DateTime Epoch = DateTime.Now;

        public static List<Utils.LuaParameter[]> UserEventQueue = new();

        private static bool _quit;
        private static SDL_Event _e;

        public static void Main(string[] args) {
            Args = args;

            Console.WriteLine($"Eight {Version}");

            if(!Directory.Exists(LuaDir)) {
                Directory.CreateDirectory(LuaDir);
            }

            Directory.SetCurrentDirectory(LuaDir);

            Init();

            Environment.Exit(0);
        }

        public static bool Init() {
            SetTickrate(DefaultTickrate);

            if (!Logic.Lua.Init()) {
                Console.WriteLine("Lua could not be initialized!");
                return false;
            }

            if (!SDL.Init()) {
                Console.WriteLine("SDL2 could not be initialized!");
                return false;
            }

            _quit = false;

            Parallel.Invoke(EventLoop, TickEmitter);

            return true;
        }

        public static void SetTickrate(int tickrate) {
            Tickrate = tickrate;
            Ticktime = 1000 / Tickrate;
        }

        public static void Resume(int n) {
            var syncTimer = new Timer {
                Enabled = true,
                AutoReset = false,
                Interval = SyncTimeout
            };

            syncTimer.Elapsed += SyncTimerHandler;

            var ok = Logic.Lua.Resume(n);
            OutOfSync = false;

            syncTimer.Stop();
            if (!ok) _quit = true;
        }

        // TODO: kill lua if this ever happens, which is very likely, i caused this at least 10 times today.
        private static void SyncTimerHandler(object sender, ElapsedEventArgs ev) {
            OutOfSync = true;
            Console.WriteLine("Warning: Lua State is out of sync!");
            Console.WriteLine("Caused after event: {0}", _e.type);
        }

        private static void EventLoop() {
            int x, y;
            var oldX = -1;
            var oldY = -1;
            var pressedMouseButtons = new List<byte>();

            using var state = Logic.Lua.State;

            foreach (var arg in Args) {
                state.PushString(arg);
            }

            Resume(Args.Length);

            while (!_quit) {
                while (!_quit && SDL_PollEvent(out _e) != 0) {
                    switch (_e.type) {
                        case SDL_QUIT:
                            Quit();
                            break;
                        case SDL_KEYDOWN:
                        case SDL_KEYUP:

                            var keyName = SDL_GetKeyName(_e.key.keysym.sym);
                            keyName = keyName.ToLower();
                            keyName = keyName.Replace(" ", "_");

                            state.PushString(_e.key.state == SDL_PRESSED ? "key_down" : "key_up");
                            state.PushString(keyName);
                            state.PushInteger((long) _e.key.keysym.sym);
                            state.PushBoolean(_e.key.repeat != 0);

                            Resume(4);

                            break;
                        case SDL_TEXTINPUT:
                            byte[] c;
                            var
                                a = _e; // "You cannot use fixed size buffers contained in unfixed expressions. Try using the 'fixed' statement" my ass
                            unsafe {
                                c = Utils.CString(a.text.text);
                            }

                            state.PushString("char");
                            state.PushBuffer(c);
                            Resume(2);

                            break;
                        case SDL_MOUSEMOTION:
                            x = _e.motion.x / WindowScale;
                            y = _e.motion.y / WindowScale;
                            if (oldX != x || oldY != y) {
                                state.PushString(pressedMouseButtons.Count > 0 ? "mouse_drag" : "mouse_hover");

                                if (pressedMouseButtons.Count > 0) state.PushInteger(pressedMouseButtons.Last());

                                state.PushInteger(x);
                                state.PushInteger(y);

                                Resume(pressedMouseButtons.Count > 0 ? 4 : 3);

                                oldX = x;
                                oldY = y;
                            }

                            break;
                        case SDL_MOUSEBUTTONDOWN:
                        case SDL_MOUSEBUTTONUP:
                            x = _e.motion.x / WindowScale;
                            y = _e.motion.y / WindowScale;

                            if (_e.button.state == SDL_PRESSED) {
                                if (!pressedMouseButtons.Contains(_e.button.button))
                                    pressedMouseButtons.Add(_e.button.button);
                            }
                            else {
                                if (pressedMouseButtons.Contains(_e.button.button))
                                    pressedMouseButtons.Remove(_e.button.button);
                            }

                            state.PushString(_e.button.state == SDL_PRESSED ? "mouse_click" : "mouse_up");
                            state.PushInteger(_e.button.button);

                            state.PushInteger(x);
                            state.PushInteger(y);

                            state.PushBoolean(_e.button.clicks != 1);

                            Resume(5);

                            break;
                        case SDL_MOUSEWHEEL:
                            x = _e.wheel.x;
                            y = _e.wheel.y;

                            if (SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED.Equals(_e.wheel.direction)) {
                                x *= -1;
                                y *= -1;
                            }

                            if (y != 0 || x != 0) {
                                state.PushString("mouse_scroll");

                                state.PushInteger(x);
                                state.PushInteger(y);

                                state.PushInteger(oldX);
                                state.PushInteger(oldY);

                                Resume(5);
                            }

                            break;
                        case SDL_USEREVENT:
                            switch (_e.user.code) {
                                case 0:
                                    SDL.DrawCanvas();
                                    state.PushString("tick");
                                    Resume(1);
                                    break;
                                case -1:
                                    // I don't really trust this code
                                    // It will probably give problems in the future
                                    try {
                                        var index = (int) _e.user.data1;
                                        var parameters = UserEventQueue[index];

                                        for (var i = 0; i < parameters.Length; i++) {
                                            var type = parameters[i].Type;
                                            var value = parameters[i].Value;

                                            switch (type) {
                                                case Lua.LuaType.Nil:
                                                    state.PushNil();
                                                    break;
                                                case Lua.LuaType.Boolean:
                                                    state.PushBoolean(Convert.ToBoolean(value));
                                                    break;
                                                case Lua.LuaType.LightUserData:
                                                    state.PushLightUserData((IntPtr) value);
                                                    break;
                                                case Lua.LuaType.Number:
                                                    state.PushNumber(Convert.ToDouble(value));
                                                    break;
                                                case Lua.LuaType.String:
                                                    if (value is byte[] v)
                                                        state.PushBuffer(v);
                                                    else if (value is string s)
                                                        state.PushString(s);
                                                    break;
                                                case Lua.LuaType.UserData:
                                                    state.PushLightUserData((IntPtr) value);
                                                    break;
                                            }
                                        }

                                        Resume(parameters.Length);
                                    }
                                    catch (Exception e) {
                                        Console.WriteLine(e);
                                    }

                                    break;
                            }

                            break;
                    }
                }
                
                SDL_Delay(1);
            }
        }

        private static void TickEmitter() {
            while (!_quit) {
                var tickEvent = new SDL_Event {
                    type = SDL_USEREVENT,
                    user = {
                        code = 0
                    }
                };
                SDL_PushEvent(ref tickEvent);
                Thread.Sleep(Ticktime);
            }
        }

        public static void PushEvent(Utils.LuaParameter[] parameters) {
            UserEventQueue.Add(parameters);
            var index = UserEventQueue.Count - 1;

            var userEvent = new SDL_Event {
                type = SDL_USEREVENT,
                user = {
                    code = -1,
                    data1 = (IntPtr) index
                }
            };

            SDL_PushEvent(ref userEvent);
        }

        public static void Quit() {
            Console.WriteLine("Quitting");
            _quit = true;

            Logic.Lua.Quit();
            SDL.Quit();
        }
    }
}