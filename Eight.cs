using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;

namespace Eight {
    internal static class Eight {
        public const string Version = "0.0.1";
        
        public const int DefaultWidth = 200;
        public const int DefaultHeight = 150;
        public const int DefaultScale = 4;

        public const int DefaultTickrate = 50;
        
        public static int WindowWidth = DefaultWidth;
        public static int WindowHeight = DefaultHeight;
        public static int WindowScale = DefaultScale;
        
        public static int Tickrate;
        public static double Ticktime;

        public static IntPtr Window = IntPtr.Zero;
        public static IntPtr Renderer = IntPtr.Zero;
        public static IntPtr Surface = IntPtr.Zero;

        public static readonly DateTime Epoch = DateTime.Now;

        private static bool _quit = false;

        private static void Main() {
            Console.WriteLine("Eight {0}", Version);

            Init();

            Quit();
        }

        private static void Init() {
            if (!SDLLogic.Init()) {
                return;
            }

            if (!LuaLogic.Init()) {
                return;
            }

            SDLLogic.CreateCanvas();

            SetTickrate(DefaultTickrate);

            Parallel.Invoke(RunEventLoop, TickTimer);
        }

        static void RunEventLoop() {
            bool isClicking = false;
            int oldx = 0;
            int oldy = 0;
            byte oldMouseButton = 0;
            int x, y;

            SDL_Event e;
            using var state = LuaLogic.sL;

            Console.WriteLine("Running event loop");

            while (!_quit) {
                while (!_quit && SDL_PollEvent(out e) != 0) {
                    switch (e.type) {
                        case SDL_QUIT:
                            _quit = true;
                            break;
                        case SDL_KEYDOWN:
                        case SDL_KEYUP:

                            string keyName = SDL_GetKeyName(e.key.keysym.sym);
                            keyName = keyName.ToLower();
                            keyName = keyName.Replace(" ", "_");

                            state.PushString(e.key.state == SDL_PRESSED ? "key_down" : "key_up");
                            state.PushInteger((long) e.key.keysym.sym);
                            state.PushString(keyName);
                            state.PushBoolean(e.key.repeat != 0);

                            Resume(4);

                            break;
                        case SDL_TEXTINPUT:
                            byte[] c;
                            unsafe {
                                c = cstr(e.text.text);
                            }

                            state.PushString("char");
                            state.PushBuffer(c);
                            Resume(2);

                            break;
                        case SDL_MOUSEMOTION:
                            x = e.motion.x / WindowScale;
                            y = e.motion.y / WindowScale;
                            if (oldx != x || oldy != y) {
                                state.PushString(isClicking ? "mouse_drag" : "mouse_hover");

                                if (isClicking) {
                                    state.PushInteger(oldMouseButton);
                                }

                                state.PushInteger(x);
                                state.PushInteger(y);

                                Resume(isClicking ? 4 : 3);

                                oldx = x;
                                oldy = y;
                            }

                            break;
                        case SDL_MOUSEBUTTONDOWN:
                        case SDL_MOUSEBUTTONUP:
                            x = e.motion.x / WindowScale;
                            y = e.motion.y / WindowScale;

                            isClicking = e.button.state == SDL_PRESSED;

                            state.PushString(e.button.state == SDL_PRESSED ? "mouse_click" : "mouse_up");
                            state.PushInteger(e.button.button);

                            oldMouseButton = e.button.button;

                            state.PushInteger(x);
                            state.PushInteger(y);

                            state.PushBoolean(e.button.clicks != 1);

                            Resume(5);

                            break;
                        case SDL_MOUSEWHEEL:
                            x = e.wheel.x;
                            y = e.wheel.y;

                            if (SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED.Equals(e.wheel.direction)) {
                                x *= -1;
                                y *= -1;
                            }

                            if (y != 0 || x != 0) {

                                state.PushString("mouse_scroll");

                                state.PushInteger(x);
                                state.PushInteger(y);

                                state.PushInteger(oldx);
                                state.PushInteger(oldy);

                                Resume(5);
                            }

                            break;
                        case SDL_USEREVENT:
                            switch (e.user.code) {
                                case 0:
                                    state.PushString("_eight_tick");
                                    Resume(1);
                                    SDLLogic.DrawCanvas();
                                    break;
                                case 1:
                                    state.PushString("timer");
                                    state.PushInteger((int) e.user.data1);
                                    Resume(2);
                                    break;
                            }

                            break;
                    }
                }

                //SDL_Delay(1);
            }
        }

        private static void TickTimer() {
            while (!_quit) {
                var ev = new SDL_Event {type = SDL_USEREVENT, user = {code = 0},};
                SDL_PushEvent(ref ev);
                Thread.Sleep(1000 / Tickrate);
            }
        }

        private static void Resume(int n) {
            if (LuaLogic.Resume(n)) return;
            _quit = true;
        }

        public static void SetTickrate(int tickrate) {
            Tickrate = tickrate;
            Ticktime = 1 / (double) tickrate;
        }

        public static void Reset(bool doQuit = false) {

            LuaLogic.IsResetting = true;
            _quit = true;
            
            LuaLogic.Reset();

            SetTickrate(DefaultTickrate);

            SDLLogic.Reset();


            if (doQuit) return;
            LuaLogic.IsResetting = false;
            Init();
        }
        
        public static void Quit() {
            Console.WriteLine("Quitting");
            Reset(true);
            SDL_Quit();
        }

        // bytechan is a monster
        private static unsafe byte[] cstr(byte* s) {
            byte* end = s;
            while (*end != 0) ++end;

            byte[] o = new byte[end - s];
            for (var i = 0; i < o.Length; i++) {
                o[i] = s[i];
            }

            return o;
        }
    }
}