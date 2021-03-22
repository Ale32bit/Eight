using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;

namespace Eight {
    public static class Eight {
        public const string Version = "Alpha 1.3.0";

        public const int DefaultWidth = 66;
        public const int DefaultHeight = 24;
        public const float DefaultScale = 2;
        public const int DefaultTickrate = 60;

        public const int CellWidth = 6;
        public const int CellHeight = 12;

        public static readonly string MainDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Eight");
        public static readonly string DataDir = Path.Combine(MainDir, "data");

        public static int RealWidth {
            get { return WindowWidth * CellWidth; }
        }

        public static int RealHeight {
            get { return WindowHeight * CellHeight; }
        }

        public static int WindowWidth;
        public static int WindowHeight;
        public static float WindowScale;

        public static int Tickrate;
        public static int Ticktime;

        public static string[] Args;

        public static int SyncTimeout = 3000;
        public static bool OutOfSync;

        public static int RebootDelay = 750;

        public static Dictionary<string, bool> Flags = new();

        public static readonly DateTime Epoch = DateTime.Now;

        private static List<Utils.LuaParameter[]> _userEventQueue = new();

        public static bool IsQuitting;

        private static bool _reset = false;
        private static SDL_Event _e;

        [STAThread]
        public static void Main(string[] args) {
            Args = args;

            Console.WriteLine($"Eight {Version}");

            if ( !Directory.Exists(DataDir) ) {
                Directory.CreateDirectory(DataDir);
            }

            if ( !File.Exists(Path.Combine(MainDir, ".installed")) ) {
                InstallOS();
                File.WriteAllText(Path.Combine(MainDir, ".installed"), "Delete this file to install the default OS on launch");
            }

            SetupFlags();

            SetTickrate(DefaultTickrate);

            Discord.Init();

            if ( !Display.Init() ) {
                Console.WriteLine("SDL2 could not be initialized!");
                return;
            }

            if ( BIOS.BootPrompt() )
                Init();

            Environment.Exit(0);
        }

        private static void SetupFlags() {
            Flags["out_of_sync_error"] = true;
            Flags["allow_rpc_change"] = true;
        }

        public static void Init() {
            if ( !Runtime.Init() ) {
                Console.WriteLine("Lua could not be initialized!");
                return;
            }

            IsQuitting = false;

            Discord.SetStatus("", "");

            while ( !IsQuitting ) {
                EventLoop();
                if ( _reset ) {
                    Console.WriteLine("Resetting environment");
                    IsQuitting = false;
                    _reset = false;
                    Runtime.Init();
                }
            }
        }

        public static void InstallOS() {
            Utils.DirectoryCopy("./Lua/lua", DataDir, true);
        }

        public static void Reset() {
            _reset = true;
            IsQuitting = true;
            Runtime.Quit();
            Display.Reset();
            SetTickrate(DefaultTickrate);
        }

        public static void SetTickrate(int tickrate) {
            Tickrate = tickrate;
            Ticktime = 1000 / Tickrate;
        }

        public static void Resume(int n) {
            if ( IsQuitting ) return;
            var syncTimer = new Timer() {
                Enabled = true,
                AutoReset = false,
                Interval = SyncTimeout
            };

            syncTimer.Elapsed += SyncTimerHandler;

            var ok = Runtime.Resume(n);
            OutOfSync = false;

            syncTimer.Stop();
            if ( !ok ) IsQuitting = true;
        }

        // TODO: improve lua state killing, gotta train those hunters
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
            var pressedKeys = new List<SDL_Keycode>();

            bool interrupted = false;
            int rebootTime = 0;

            using var state = Runtime.State;

            foreach ( var arg in Args ) {
                state.PushString(arg);
            }

            Resume(Args.Length);

            double pfreq = SDL_GetPerformanceFrequency();
            double ptime = 0;
            ulong last = SDL_GetPerformanceCounter();
            ulong now = last;

            while ( !IsQuitting ) {
                while ( !IsQuitting && SDL_PollEvent(out _e) != 0 ) {
                    switch ( _e.type ) {
                        case SDL_QUIT:
                            Quit();
                            break;
                        case SDL_KEYDOWN:
                        case SDL_KEYUP:

                            if ( _e.key.state == SDL_PRESSED ) {
                                if ( !pressedKeys.Contains(_e.key.keysym.sym) )
                                    pressedKeys.Add(_e.key.keysym.sym);
                            } else {
                                if ( pressedKeys.Contains(_e.key.keysym.sym) )
                                    pressedKeys.Remove(_e.key.keysym.sym);
                            }

                            var keyName = SDL_GetKeyName(_e.key.keysym.sym);
                            keyName = keyName.ToLower();
                            keyName = keyName.Replace(" ", "_");

                            state.PushString(_e.key.state == SDL_PRESSED ? "key_down" : "key_up");
                            state.PushString(keyName);
                            state.PushInteger((long)_e.key.keysym.sym);
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
                            x = (int)(_e.motion.x / WindowScale);
                            y = (int)(_e.motion.y / WindowScale);
                            if ( oldX != x || oldY != y ) {
                                state.PushString(pressedMouseButtons.Count > 0 ? "mouse_drag" : "mouse_move");

                                if ( pressedMouseButtons.Count > 0 ) state.PushInteger(pressedMouseButtons.Last());

                                state.PushInteger(x);
                                state.PushInteger(y);

                                Resume(pressedMouseButtons.Count > 0 ? 4 : 3);

                                oldX = x;
                                oldY = y;
                            }

                            break;
                        case SDL_MOUSEBUTTONDOWN:
                        case SDL_MOUSEBUTTONUP:
                            x = (int)(_e.motion.x / WindowScale);
                            y = (int)(_e.motion.y / WindowScale);

                            if ( _e.button.state == SDL_PRESSED ) {
                                if ( !pressedMouseButtons.Contains(_e.button.button) )
                                    pressedMouseButtons.Add(_e.button.button);
                            } else {
                                if ( pressedMouseButtons.Contains(_e.button.button) )
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

                            if ( SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED.Equals(_e.wheel.direction) ) {
                                x *= -1;
                                y *= -1;
                            }

                            if ( y != 0 || x != 0 ) {
                                state.PushString("mouse_scroll");

                                state.PushInteger(x);
                                state.PushInteger(y);

                                state.PushInteger(oldX);
                                state.PushInteger(oldY);

                                Resume(5);
                            }

                            break;
                        case SDL_JOYAXISMOTION:
                            var id = _e.jaxis.which;
                            var axis = _e.jaxis.axis;
                            var axisValue = _e.jaxis.axisValue;

                            state.PushString("joyaxis_motion");
                            state.PushNumber(id);
                            state.PushNumber(axis);
                            state.PushNumber(axisValue);
                            Resume(4);

                            break;
                        case SDL_USEREVENT:
                            switch ( _e.user.code ) {
                                case -1:
                                    // I don't really trust this code
                                    // It will probably give problems in the future
                                    try {
                                        var index = (int)_e.user.data1;
                                        var parameters = _userEventQueue[index];

                                        for ( var i = 0; i < parameters.Length; i++ ) {
                                            var type = parameters[i].Type;
                                            var value = parameters[i].Value;

                                            switch ( type ) {
                                                case LuaType.Nil:
                                                    state.PushNil();
                                                    break;
                                                case LuaType.Boolean:
                                                    state.PushBoolean(Convert.ToBoolean(value));
                                                    break;
                                                case LuaType.LightUserData:
                                                    state.PushLightUserData((IntPtr)value);
                                                    break;
                                                case LuaType.Number:
                                                    state.PushNumber(Convert.ToDouble(value));
                                                    break;
                                                case LuaType.String:
                                                    if ( value is byte[] v )
                                                        state.PushBuffer(v);
                                                    else if ( value is string s )
                                                        state.PushString(s);
                                                    break;
                                                case LuaType.UserData:
                                                    state.PushLightUserData((IntPtr)value);
                                                    break;
                                            }
                                        }

                                        Resume(parameters.Length);
                                    } catch ( Exception e ) {
                                        Console.WriteLine(e);
                                    }

                                    break;
                            }

                            break;
                    }
                }

                if ( (ptime * 1000) >= Ticktime ) {
                    ptime = 0;
                    if ( !IsQuitting ) {
                        if ( state.Status == LuaStatus.Yield ) {
                            // If pressed ^C
                            if ( pressedKeys.Contains(SDL_Keycode.SDLK_LCTRL) ) {
                                if ( pressedKeys.Contains(SDL_Keycode.SDLK_c) ) {
                                    if ( !interrupted ) {
                                        PushEvent(new Utils.LuaParameter[] {
                                        new() {
                                            Type = LuaType.String,
                                            Value = "interrupt",
                                        }
                                    });
                                        interrupted = true;
                                    }
                                } else {
                                    interrupted = false;
                                }

                                if ( pressedKeys.Contains(SDL_Keycode.SDLK_r) ) {
                                    rebootTime += Ticktime;
                                    if ( rebootTime >= RebootDelay ) {
                                        Reset();
                                    }
                                } else {
                                    rebootTime = 0;
                                }
                            }

                            Display.Update();
                            Display.RenderScreen();
                        }
                    }
                }
                SDL_Delay(1);
                now = SDL_GetPerformanceCounter();
                ptime += (now - last) / pfreq;
                last = now;
            }
        }

        public static void PushEvent(Utils.LuaParameter[] parameters) {
            _userEventQueue.Add(parameters);
            var index = _userEventQueue.Count - 1;

            var userEvent = new SDL_Event {
                type = SDL_USEREVENT,
                user = {
                    code = -1,
                    data1 = (IntPtr) index
                }
            };

            SDL_PushEvent(ref userEvent);
        }

        public static void Crash(params string[] messages) {
            BIOS.ResetScreen();

            BIOS.X = 0;
            BIOS.Y = 0;

            foreach ( var msg in messages ) {
                BIOS.Print(msg);
            }

            BIOS.Render();

            while ( SDL_WaitEvent(out var ev) != 0 ) {
                if ( ev.type == SDL_QUIT ) {
                    break;
                }
            }

            Quit();
        }

        public static void Quit() {
            Console.WriteLine("Quitting");
            IsQuitting = true;

            Runtime.Quit();
            Display.Quit();
        }
    }
}