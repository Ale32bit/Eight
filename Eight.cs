using System;
using System.Threading;
using System.Threading.Tasks;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;

namespace Eight {
    internal static class Eight {
        public const string Version = "0.0.1";

        public static int WindowWidth = 200;
        public static int WindowHeight = 150;
        public static int WindowScale = 4;

        public static int Tickrate;
        public static double Ticktime;

        public static IntPtr Window = IntPtr.Zero;
        public static IntPtr Renderer = IntPtr.Zero;
        public static IntPtr Surface = IntPtr.Zero;

        private static bool _quit = false;

        private static void Main() {
            Console.WriteLine("Eight {0}", Version);

            if (!SDLLogic.Init()) {
                return;
            }

            if (!LuaLogic.Init()) {
                return;
            }

            SDLLogic.CreateCanvas();
            
            SetTickrate(60);
            
            RunEventLoop();
            
            Quit();
        }


        static void RunEventLoop() {
            bool isClicking = false;
            int oldx = 0;
            int oldy = 0;
            byte oldMouseButton = 0;
            int x, y;

            uint lastTime = 0;

            SDL_Event e;
            using var state = LuaLogic.sL;

            Console.WriteLine("Running event loop");

            while (!_quit) {
                while (SDL_WaitEventTimeout(out e, (int) Ticktime * 1000) != 0) {
                    switch (e.type) {
                        case SDL_QUIT:
                            _quit = true;
                            break;
                        case SDL_KEYDOWN:
                        case SDL_KEYUP:

                            state.PushString(e.key.state == SDL_PRESSED ? "key_down" : "key_up");
                            state.PushInteger((long) e.key.keysym.sym);
                            state.PushBoolean(e.key.repeat != 0);

                            Resume(3);

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

                            state.PushString("mouse_wheel");

                            state.PushInteger(x);
                            state.PushInteger(y);

                            state.PushInteger(oldx);
                            state.PushInteger(oldy);

                            Resume(5);

                            break;
                    }
                }

                var currentTime = SDL_GetTicks();
                if (currentTime > lastTime + (1000 / Tickrate)) {
                    state.PushString("_eight_tick");
                    Resume(1);
                    SDLLogic.DrawCanvas();
                    lastTime = currentTime;
                }
                
                SDL_Delay(1);

            }
        }

        private static void Resume(int n) {
            if (LuaLogic.Resume(n)) return;
            Console.WriteLine("reached exception");
            _quit = true;
        }

        public static void SetTickrate(int tickrate) {
            Tickrate = tickrate;
            Ticktime = 1 / (double) tickrate;
        }

        public static void Quit() {
            Console.WriteLine("Quitting");
            SDL_DestroyRenderer(Renderer);
            SDL_DestroyWindow(Window);
            SDL_Quit();
        }
    }
}