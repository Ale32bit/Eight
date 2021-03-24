using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static SDL2.SDL;

namespace Eight {
    class BIOS {
        struct BIOSOption {
            public Action cb;
            public string name;
        }

        struct Config {
            public int Width;
            public int Height;
            public float Scale;
        }

        public static int X = 0;
        public static int Y = 0;

        private static bool quitBios = false;
        private static bool resume = true;

        private static int selection = 0;
        private static string configPath = Path.Combine(Eight.MainDir, "config.xml");
        private static Config biosConfig;

        private static Config LoadConfig() {
            XmlDocument doc = new();
            doc.Load(configPath);

            XmlNode widthNode = doc.DocumentElement.SelectSingleNode("/config/width");
            XmlNode heightNode = doc.DocumentElement.SelectSingleNode("/config/height");
            XmlNode scaleNode = doc.DocumentElement.SelectSingleNode("/config/scale");

            return new Config {
                Width = int.Parse(widthNode.InnerText),
                Height = int.Parse(heightNode.InnerText),
                Scale = float.Parse(scaleNode.InnerText),
            };
        }

        private static void SaveConfig(Config config) {
            XmlDocument doc = new();

            XmlElement root = doc.CreateElement("config");
            doc.AppendChild(root);

            XmlNode widthNode = doc.CreateNode(XmlNodeType.Element, "width", null);
            widthNode.InnerText = config.Width.ToString();
            doc.DocumentElement.AppendChild(widthNode);

            XmlNode heightNode = doc.CreateNode(XmlNodeType.Element, "height", null);
            heightNode.InnerText = config.Height.ToString();
            doc.DocumentElement.AppendChild(heightNode);

            XmlNode scaleNode = doc.CreateNode(XmlNodeType.Element, "scale", null);
            scaleNode.InnerText = config.Scale.ToString();
            doc.DocumentElement.AppendChild(scaleNode);


            doc.Save(configPath);

        }

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

        public static void Write(string msg) {
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
        }

        public static void Print(string msg) {
            Write(msg);
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
                name = "Setup screen size",
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
                name = "width",
                cb = () => {
                    X = 0;
                    Y++;
                    Write("Type new width: ");
                    var width = int.Parse(ReadLine(4, @"^\d+$"));
                    biosConfig.Width = width;
                },
            },
            new() {
                name = "height",
                cb = () => {
                    X = 0;
                    Y++;
                    Write("Type new height: ");
                    var height = int.Parse(ReadLine(4, @"^\d+$"));
                    biosConfig.Height = height;
                },
            },
            new() {
                name = "scale",
                cb = () => {
                    X = 0;
                    Y++;
                    Write("Type new scale: ");
                    var scale = float.Parse(ReadLine(4, @"^\d+$"));
                    biosConfig.Scale = scale;
                },
            },
            new() {
                name = $"Reset ({Eight.DefaultWidth} x {Eight.DefaultHeight} x {Eight.DefaultScale})",
                cb = () => {
                    biosConfig.Width = Eight.DefaultWidth;
                    biosConfig.Height = Eight.DefaultHeight;
                    biosConfig.Scale = Eight.DefaultScale;
                },
            },
            new() {
                name = "Save",
                cb = () => {
                    SaveConfig(biosConfig);
                },
            },
            new() {
                name = "Back",
                cb = () => {
                    quitScreenSetup = true;
                },
            }
        };

        private static void ScreenSizeSetup() {
            var drawMenu = new Action(() => {
                // update screenSetupOptions names in case size changed

                screenSetupOptions[0].name = $"Default width ({biosConfig.Width})"; // width
                screenSetupOptions[1].name = $"Default height ({biosConfig.Height})"; // height
                screenSetupOptions[2].name = $"Default scale ({biosConfig.Scale})"; // scale

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
            X = 0;
            Y = 0;

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

            // Load config

            if ( !File.Exists(configPath) ) {
                SaveConfig(new Config {
                    Width = Eight.DefaultWidth,
                    Height = Eight.DefaultHeight,
                    Scale = Eight.DefaultScale,
                });
            }

            biosConfig = LoadConfig();

            Discord.SetStatus("Booting up");

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
                Discord.SetStatus("In setup menu", "");
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

            if ( resume ) {
                Display.SetScreenSize(biosConfig.Width, biosConfig.Height, biosConfig.Scale);
            }

            return resume;
        }

        public static string ReadLine(int? maxLen, string regex = "*") {
            string input = "";
            int ox = X;
            int oy = Y;

            var redraw = new Action(() => {
                X = ox;
                Y = oy;
                Print(input + " ");
                Render();
            });

            Render();
            bool exit = false;
            while ( SDL_WaitEvent(out var _ev) != 0 && !exit ) {
                if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                    Eight.Quit();
                    break;
                }

                if ( _ev.type == SDL_EventType.SDL_TEXTINPUT ) {
                    byte[] c;
                    unsafe {
                        c = Utils.CString(_ev.text.text);
                    }
                    var chunk = Encoding.UTF8.GetString(c);

                    if ( Regex.IsMatch(chunk, regex) ) {
                        input += chunk;
                    }

                    if ( maxLen != null ) {
                        input = input.Substring(0, Math.Min((int)maxLen, input.Length));
                    }
                    redraw();
                }

                if ( _ev.type == SDL_EventType.SDL_KEYDOWN ) {
                    switch ( _ev.key.keysym.sym ) {
                        case SDL_Keycode.SDLK_RETURN:
                            exit = true;
                            break;
                        case SDL_Keycode.SDLK_BACKSPACE:
                            if ( input.Length > 0 ) {
                                input = input.Substring(0, input.Length - 1);
                                redraw();
                            }
                            break;
                    }
                }
            }

            return input;
        }
    }
}
