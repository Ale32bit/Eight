using System;
using System.Diagnostics;
using System.Text;
using static SDL2.SDL;

namespace Eight {
    class BIOS {
        struct BIOSOption {
            public Action cb;
            public string name;
        }

        struct ScreenSizeOption {
            public Action cb;
            public string name;
        }

        public static int X = 0;
        public static int Y = 0;

        private static bool quitBios = false;
        private static bool resume = true;

        private static int selection = 0;

        public static void Render() {
            Display.Update();
            Display.RenderScreen();
        }

        public static void ResetScreen() {
            Module.ScreenText.ForegroundColor = 0xffffff;
            Module.ScreenText.BackgroundColor = 0x000000;
            Display.ResetScreenSize();
            Module.ScreenText.ClearScreen(true);
        }

        public static void Print(string msg) {
            for ( int i = 0; i < msg.Length; i++ ) {
                char ch = msg[i];
                if ( ch == '\t' ) {
                    X += 2;
                } else if ( ch == '\n' ) {
                    Y++;
                    X = 0;
                } else {
                    Module.ScreenText.DrawChar(ch, X, Y, Module.ScreenText.ForegroundColor, Module.ScreenText.BackgroundColor);
                    X++;
                }
            }
            X = 0;
            Y++;
        }

        public static void CenterPrint(string msg) {
            X = (Eight.WindowWidth - msg.Length) / 2;
            Print(msg);
        }

        private static void OpenFolder(string path) {
            switch ( Environment.OSVersion.Platform ) {
                case PlatformID.Win32NT:
                    Process.Start("explorer.exe", path);
                    break;
                case PlatformID.Unix:
                    Process.Start("xdg-open", path);
                    break;
            }
        }

        private static BIOSOption[] biosOptions = {
            new() {
                name = "Open data directory",
                cb = () => {
                    OpenFolder(Eight.DataDir);
                },
            },

            new() {
                name = "Install default OS",
                cb = Eight.InstallOS,
            },

            new() {
                name = "Setup screen size (WIP)",
                cb = ScreenSizeSetup,
            },

            new() {
                name = "Continue",
                cb = () => {
                    quitBios = true;
                    resume = true;
                },
            },

            new() {
                name = "Quit",
                cb = () => {
                    quitBios = true;
                    resume = false;
                    Eight.Quit();
                },
            }
        };

        static bool quitScreenSetup = false;
        private static BIOSOption[] screenSetupOptions = {
            new() {
                name = $"Default width ({Eight.WindowWidth})",

            },
            new() {
                name = $"Default height ({Eight.WindowHeight})",

            },
            new() {
                name = $"Default scale ({Eight.WindowScale})",

            },
            new() {
                name = $"Reset ({Eight.DefaultWidth}x{Eight.DefaultHeight}x{Eight.DefaultScale})",
            },
            new() {
                name = "Save",
            },
            new() {
                name = "Back",
            }
        };

        private static void ScreenSizeSetup() {
            var drawMenu = new Action(() => {
                X = 0;
                Y = 0;

                ResetScreen();
                CenterPrint("Screen Size Setup Menu");
                Y = 2;
                for ( int i = 0; i < screenSetupOptions.Length; i++ ) {
                    if ( selection == i ) {
                        CenterPrint($"[ {screenSetupOptions[i].name} ]");
                    } else {
                        CenterPrint($"{screenSetupOptions[i].name}");
                    }
                }
                Render();
            });

            selection = 0;
            quitScreenSetup = false;

            drawMenu();
            while ( SDL_WaitEvent(out var _ev) != 0 ) {

                if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                    Eight.Quit();
                    quitBios = true;
                    return;
                }

                if ( _ev.type == SDL_EventType.SDL_KEYDOWN ) {
                    if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_DOWN ) {
                        selection++;
                    } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_UP ) {
                        selection--;
                    } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_RETURN ) {
                        screenSetupOptions[selection].cb?.Invoke();
                        if ( quitScreenSetup ) {
                            selection = 0;
                            break;
                        }
                    }

                    if ( selection < 0 ) selection = screenSetupOptions.Length - 1;
                    if ( selection >= screenSetupOptions.Length ) selection = 0;

                    drawMenu();
                }
            }
        }

        public static bool BootPrompt() {
            var drawMenu = new Action(() => {
                X = 0;
                Y = 0;

                ResetScreen();
                CenterPrint("Eight Setup Menu");
                Y = 2;
                for ( int i = 0; i < biosOptions.Length; i++ ) {
                    if ( selection == i ) {
                        CenterPrint($"[ {biosOptions[i].name} ]");
                    } else {
                        CenterPrint($"{biosOptions[i].name}");
                    }
                }
                Render();
            });

            Print("Eight " + Eight.Version);

            Y = Eight.WindowHeight - 1;

            CenterPrint("Press F2 to enter setup");

            Render();

            bool pressedF2 = false;

            SDL_AddTimer(1500, (interval, param) => {
                SDL_Event ev = new();

                ev.type = SDL_EventType.SDL_USEREVENT;
                ev.user.data1 = (IntPtr)0;

                SDL_PushEvent(ref ev);
                return interval;
            }, IntPtr.Zero);

            Module.Audio.InitAudio();

            while ( SDL_WaitEvent(out var _ev) != 0 ) {
                if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                    Eight.Quit();
                    return false;
                }
                if ( _ev.type == SDL_EventType.SDL_KEYDOWN ) {
                    if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_F2 ) {
                        pressedF2 = true;
                        break;
                    }
                }
                if ( _ev.type == SDL_EventType.SDL_USEREVENT ) {
                    if ( _ev.user.data1 == (IntPtr)0 ) break;
                }
            }

            if ( pressedF2 ) {
                drawMenu();
                while ( SDL_WaitEvent(out var _ev) != 0 ) {

                    if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                        Eight.Quit();
                        return false;
                    }

                    if ( _ev.type == SDL_EventType.SDL_KEYDOWN ) {
                        if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_DOWN ) {
                            selection++;
                        } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_UP ) {
                            selection--;
                        } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_RETURN ) {
                            biosOptions[selection].cb?.Invoke();
                            if ( quitBios ) break;
                        }

                        if ( selection < 0 ) selection = biosOptions.Length - 1;
                        if ( selection >= biosOptions.Length ) selection = 0;

                        drawMenu();
                    }
                }
            }
            return resume;
        }
    }
}
