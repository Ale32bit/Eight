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

        public static int X = 0;
        public static int Y = 0;

        private static bool quitBios = false;
        private static bool resume = true;

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
                name = "Setup screen size (WIP)"
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

        private static void DrawMenu() {
            X = 0;
            Y = 0;

            ResetScreen();
            Print("Eight BIOS");
            for ( int i = 0; i < biosOptions.Length; i++ ) {
                Print($"[{i + 1}] {biosOptions[i].name}");
            }
            Render();
        }

        public static bool BootPrompt() {
            Print("Eight " + Eight.Version);

            Y = Eight.WindowHeight - 1;

            Print("Press F2 to enter BIOS");

            Render();

            bool pressedF2 = false;

            SDL_AddTimer(1500, (interval, param) => {
                SDL_Event ev = new();

                ev.type = SDL_EventType.SDL_USEREVENT;
                ev.user.data1 = (IntPtr)0;

                SDL_PushEvent(ref ev);
                return interval;
            }, IntPtr.Zero);

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
                DrawMenu();
                while ( SDL_WaitEvent(out var _ev) != 0 ) {

                    if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                        Eight.Quit();
                        return false;
                    }
                    if ( _ev.type == SDL_EventType.SDL_TEXTINPUT ) {

                        unsafe {
                            if ( int.TryParse(Encoding.ASCII.GetString(Utils.CString(_ev.text.text)), out var pick) ) {
                                if ( pick >= 1 && (pick - 1) < biosOptions.Length ) {
                                    biosOptions[pick - 1].cb?.Invoke();
                                    if ( quitBios ) break;
                                    DrawMenu();
                                }
                            }
                        }
                    }

                }
            }
            return resume;
        }
    }
}
