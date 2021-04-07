using System;
using System.Collections.Generic;
using System.IO;
using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;
using System.Runtime.InteropServices;

namespace Eight {
    public static class Eight {
        public const string Version = "Alpha 1.4.0";

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

        public static int RebootDelay = 750;

        public static Dictionary<string, bool> Flags = new();

        public static readonly DateTime Epoch = DateTime.Now;

        public static bool IsQuitting;

        private static bool _reset = false;

        [STAThread]
        public static void Main(string[] args) {
            Args = args;

            Console.WriteLine($"Eight {Version}");

            if ( Display.Init() ) {
                Discord.Init();
                Init();

                Environment.Exit(0);
            } else {
                Console.WriteLine("SDL2 could not be initialized!");
                return;
            }
        }

        public static void Init() {
            IsQuitting = false;
            while ( !IsQuitting ) {
                if ( !Directory.Exists(DataDir) ) {
                    Directory.CreateDirectory(DataDir);
                }

                if ( !File.Exists(Path.Combine(MainDir, ".installed")) ) {
                    InstallOS();
                    File.WriteAllText(Path.Combine(MainDir, ".installed"), "Delete this file to install the default OS on launch");
                }

                SetupFlags();

                SetTickrate(DefaultTickrate);

                Discord.SetStatus("", "");

                // BIOS waits too little time after reboots... why?!
                if ( BIOS.BootPrompt() ) {
                    if ( !Runtime.Init() ) {
                        Console.WriteLine("Lua could not be initialized!");
                        return;
                    }

                    Event.Run();

                } else {
                    break;
                }

                if ( _reset ) {
                    IsQuitting = false;
                    _reset = false;
                }
            }
        }

        /*public static void LegacyInit() {
            if ( !BIOS.BootPrompt() ) return;

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
                    if ( BIOS.BootPrompt() ) {
                        Runtime.Init();
                    } else {
                        break;
                    }
                }
            }
        }
        */

        private static void SetupFlags() {
            Flags["out_of_sync_error"] = true;
            Flags["allow_rpc_change"] = true;
        }

        public static void InstallOS() {
            Utils.DirectoryCopy("./Lua/lua", DataDir, true);
        }

        public static void SetTickrate(int tickrate) {
            Tickrate = tickrate;
            Ticktime = 1000 / Tickrate;
        }

        // Is this Windows only?
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public static void ShowConsole(bool show) {
            var handle = GetConsoleWindow();
            if ( show ) {
                ShowWindow(handle, SW_SHOW);
            } else {
                ShowWindow(handle, SW_HIDE);
            }

            SDL_RaiseWindow(Display.Window);
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

        public static void Reset() {
            _reset = true;
            IsQuitting = true;
            Runtime.Quit();
            Display.Reset();
        }

        public static void Quit() {
            Console.WriteLine("Quitting");
            IsQuitting = true;

            Runtime.Quit();
            Display.Quit();
            Discord.Dispose();
        }
    }
}