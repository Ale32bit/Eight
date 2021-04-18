using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static SDL2.SDL;

namespace Eight {
    class BIOS {
        public struct PromptOption {
            public Func<bool> cb;
            public string name;
        }

        public struct Config {
            public int Width;
            public int Height;
            public float Scale;
            public bool EnableInternet;
            public bool ShowConsole;
            public bool EnableDiscordRPC;
        }

        public static int X = 0;
        public static int Y = 0;

        private static bool resume = true;

        private static string configPath = Path.Combine(Eight.MainDir, "config.xml");
        public static Config biosConfig;

        private static Config LoadConfig() {
            Console.WriteLine("Loading configuration...");

            XmlDocument doc = new();
            doc.Load(configPath);

            XmlNode widthNode = doc.DocumentElement.SelectSingleNode("/config/width");
            XmlNode heightNode = doc.DocumentElement.SelectSingleNode("/config/height");
            XmlNode scaleNode = doc.DocumentElement.SelectSingleNode("/config/scale");
            XmlNode enableHttpNode = doc.DocumentElement.SelectSingleNode("/config/enable_internet");
            XmlNode showConsoleNode = doc.DocumentElement.SelectSingleNode("/config/show_console");
            XmlNode enableDiscordRPCNode = doc.DocumentElement.SelectSingleNode("/config/enable_discord_rpc");

            int width;
            if ( widthNode == null || !int.TryParse(widthNode.InnerText, out width) ) width = Eight.DefaultWidth;

            int height;
            if ( heightNode == null || !int.TryParse(heightNode.InnerText, out height) ) height = Eight.DefaultHeight;

            float scale;
            if ( scaleNode == null || !float.TryParse(scaleNode.InnerText, out scale) ) scale = Eight.DefaultScale;

            bool enableHttp;
            if ( enableHttpNode == null || !bool.TryParse(enableHttpNode.InnerText, out enableHttp) ) enableHttp = true;

            bool showConsole;
            if ( showConsoleNode == null || !bool.TryParse(showConsoleNode.InnerText, out showConsole) ) showConsole = false;

            bool enableDiscordRPC;
            if ( enableDiscordRPCNode == null || !bool.TryParse(enableDiscordRPCNode.InnerText, out enableDiscordRPC) ) enableDiscordRPC = false;

            return new Config {
                Width = width,
                Height = height,
                Scale = scale,
                EnableInternet = enableHttp,
                ShowConsole = showConsole,
                EnableDiscordRPC = enableDiscordRPC,
            };
        }

        private static void SaveConfig(Config config) {
            Console.WriteLine("Saving configuration...");

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

            XmlNode enableHttpNode = doc.CreateNode(XmlNodeType.Element, "enable_internet", null);
            enableHttpNode.InnerText = config.EnableInternet.ToString();
            doc.DocumentElement.AppendChild(enableHttpNode);

            XmlNode showConsoleNode = doc.CreateNode(XmlNodeType.Element, "show_console", null);
            showConsoleNode.InnerText = config.ShowConsole.ToString();
            doc.DocumentElement.AppendChild(showConsoleNode);

            XmlNode enableDiscordRPCNode = doc.CreateNode(XmlNodeType.Element, "enable_discord_rpc", null);
            enableDiscordRPCNode.InnerText = config.EnableDiscordRPC.ToString();
            doc.DocumentElement.AppendChild(enableDiscordRPCNode);

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

#nullable enable
        public static void PromptMenu(PromptOption[] options, string title, Action? update = null) {
            int selection = 0;

            var drawMenu = new Action(() => {
                update?.Invoke();
                X = 0;
                Y = 0;

                ResetScreen();
                CenterPrint(title);
                Y = 2;
                for ( int i = 0; i < options.Length; i++ ) {
                    if ( selection == i ) {
                        CenterPrint($"[ {options[i].name} ]");
                    } else {
                        CenterPrint($"{options[i].name}");
                    }
                }
                Render();
            });

            drawMenu();

            while ( SDL_WaitEvent(out var _ev) != 0 ) {

                if ( _ev.type == SDL_EventType.SDL_QUIT ) {
                    Eight.Quit();
                    return;
                }

                if ( _ev.type == SDL_EventType.SDL_KEYDOWN ) {
                    if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_DOWN ) {
                        selection++;
                    } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_UP ) {
                        selection--;
                    } else if ( _ev.key.keysym.sym == SDL_Keycode.SDLK_RETURN ) {
                        if ( options[selection].cb != null && options[selection].cb.Invoke() ) {
                            break;
                        }
                    }

                    if ( selection < 0 ) selection = options.Length - 1;
                    if ( selection >= options.Length ) selection = 0;

                    drawMenu();
                }
            }
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

        private static PromptOption[] configSetupOptions = {
            new() {
                name = "width",
                cb = () => {
                    X = 0;
                    Y++;
                    Write("Type new width: ");
                    var width = int.Parse(ReadLine(4, @"^\d+$"));
                    biosConfig.Width = width;
                    return false;
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
                    return false;
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
                    return false;

                },
            },
            new() {
                name = $"Reset ({Eight.DefaultWidth} x {Eight.DefaultHeight} x {Eight.DefaultScale})",
                cb = () => {
                    biosConfig.Width = Eight.DefaultWidth;
                    biosConfig.Height = Eight.DefaultHeight;
                    biosConfig.Scale = Eight.DefaultScale;
                    return false;

                },
            },
            new() {
                name = "enable_internet",
                cb = () => {
                    biosConfig.EnableInternet = !biosConfig.EnableInternet;
                    return false;
                },
            },
            new() {
                name = "show_console",
                cb = () => {
                    biosConfig.ShowConsole = !biosConfig.ShowConsole;
                    Eight.ShowConsole(biosConfig.ShowConsole);
                    return false;
                }
            },
            new() {
                name = "enable_discord_rpc",
                cb = () => {
                    if(biosConfig.EnableDiscordRPC) {
                        biosConfig.EnableDiscordRPC = false;
                        Discord.Dispose();
                    } else {
                        biosConfig.EnableDiscordRPC = true;
                        Discord.Init();
                        Discord.SetStatus("In setup menu");
                    }
                    return false;
                }
            },
            new() {
                name = "Save",
                cb = () => {
                    SaveConfig(biosConfig);
                    return false;
                },
            },
            new() {
                name = "Back",
                cb = () => {
                    return true;

                },
            }
        };

        private static PromptOption[] biosOptions = {
            new() {
                name = "Open data directory",
                cb = () => {
                    OpenFolder(Eight.DataDir);
                    return false;
                },
            },

            new() {
                name = "Install default OS",
                cb = () => {
                    Eight.InstallOS();
                    return false;
                },
            },

            new() {
                name = "Configuration setup",
                cb = () => {
                    PromptMenu(configSetupOptions, "Configuration Setup", () => {
                        configSetupOptions[0].name = $"Default width ({biosConfig.Width})"; // width
                        configSetupOptions[1].name = $"Default height ({biosConfig.Height})"; // height
                        configSetupOptions[2].name = $"Default scale ({biosConfig.Scale})"; // scale
                        configSetupOptions[4].name = "Toggle internet (" + (biosConfig.EnableInternet ? "Enabled" : "Disabled") + ")"; // change enable_internet name
                        configSetupOptions[5].name = "Toggle console (" + (biosConfig.ShowConsole ? "Enabled" : "Disabled") + ")"; // change show_console name
                        configSetupOptions[6].name = "Toggle Discord RPC (" + (biosConfig.EnableDiscordRPC ? "Enabled" : "Disabled") + ")"; // change enable_discord_rpc name
                    });

                    return false;
                },
            },
            new() {
                name = "Continue",
                cb = () => {
                    resume = true;
                    return true;
                },
            },

            new() {
                name = "Quit",
                cb = () => {
                    resume = false;
                    return true;
                },
            }
        };

        public static bool BootPrompt() {
            X = 0;
            Y = 0;
            resume = true;

            // Load config

            if ( !File.Exists(configPath) ) {
                SaveConfig(new Config {
                    Width = Eight.DefaultWidth,
                    Height = Eight.DefaultHeight,
                    Scale = Eight.DefaultScale,
                    EnableInternet = true,
                    ShowConsole = false,
                    EnableDiscordRPC = true,
                });
            }

            biosConfig = LoadConfig();
            SaveConfig(biosConfig); // In case new configs are added

            Eight.ShowConsole(biosConfig.ShowConsole);

            if ( biosConfig.EnableDiscordRPC ) {
                Discord.Init();
                Discord.SetStatus("Booting up");
            }

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
                PromptMenu(biosOptions, "Eight Setup Menu");
            }

            if ( resume ) {
                Display.SetScreenSize(biosConfig.Width, biosConfig.Height, biosConfig.Scale);
            }

            return resume;
        }
    }
}
