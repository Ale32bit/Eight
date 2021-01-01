using System;
using static SDL2.SDL;
using static SDL2.SDL_image;
using static SDL2.SDL_ttf;

namespace Eight.Logic {
    public class SDL {
        public static IntPtr Window = IntPtr.Zero;
        public static IntPtr Renderer = IntPtr.Zero;
        public static IntPtr Surface = IntPtr.Zero;

        public static bool Dirty = true;

        public static ulong[] TextGrid;

        public static IntPtr TextFont;

        public static bool Init() {
            Console.WriteLine("Initializing SDL...");

            SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (SDL_Init(SDL_INIT_EVENTS | SDL_INIT_VIDEO | SDL_INIT_AUDIO) != 0) {
                Console.WriteLine("SDL_Init Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            TTF_Init();
            IMG_Init(IMG_InitFlags.IMG_INIT_PNG);

            ResetScreenSize();

            Console.WriteLine("Creating window...");
            Window = SDL_CreateWindow("Eight " + Eight.Version, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                Eight.RealWidth * Eight.WindowScale,
                Eight.RealHeight * Eight.WindowScale,
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

            if (Window == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateWindow Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            var icon = IMG_Load("../icon.png");
            if (icon != IntPtr.Zero) {
                SDL_SetWindowIcon(Window, icon);
            } else {
                Console.WriteLine("Failed to load icon.png");
            }

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "0");
            SDL_SetRenderDrawBlendMode(Renderer, SDL_BlendMode.SDL_BLENDMODE_NONE);

            Console.WriteLine("Creating renderer...");
            Renderer = SDL_CreateRenderer(Window, -1,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (Renderer == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateRenderer Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            TextFont = TTF_OpenFont("../Assets/tewi.ttf", 11);

            if (TextFont == IntPtr.Zero) throw new Exception(SDL_GetError());

            return true;
        }

        public static void CreateScreen() {
            if (Surface != IntPtr.Zero) {
                SDL_FreeSurface(Surface);
                Surface = IntPtr.Zero;
            }

            Surface = SDL_CreateRGBSurface(0, Eight.RealWidth,
                Eight.RealHeight, 32,
                0xff000000,
                0x00ff0000,
                0x0000ff00,
                0x000000ff);

            if (Surface == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateRGBSurface() failed: " + SDL_GetError());
                Eight.Quit();
            }

            TextGrid = new ulong[Eight.WindowWidth * Eight.WindowHeight];

            Dirty = true;
        }

        private static void UpdateScreen() {
            SDL_SetWindowSize(Window,
                Eight.RealWidth * Eight.WindowScale,
                Eight.RealHeight * Eight.WindowScale
            );

            SDL_SetWindowPosition(Window, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED);

            CreateScreen();
        }


        public static void SetScreenSize(int width, int height, int scale) {
            Eight.WindowWidth = width;
            Eight.WindowHeight = height;
            Eight.WindowScale = scale;

            UpdateScreen();
        }

        public static void ResetScreenSize() {
            SetScreenSize(Eight.DefaultWidth, Eight.DefaultHeight, Eight.DefaultScale);
        }

        public static void RenderScreen() {
            if (!Dirty) return;

            var sTexture = SDL_CreateTextureFromSurface(Renderer, Surface);

            SDL_RenderClear(Renderer);

            SDL_RenderCopy(Renderer, sTexture, IntPtr.Zero, IntPtr.Zero);

            SDL_RenderPresent(Renderer);

            SDL_DestroyTexture(sTexture);

            Dirty = false;
        }

        public static void Quit() {
            if (Surface != IntPtr.Zero)
                SDL_FreeSurface(Surface);

            SDL_DestroyRenderer(Renderer);
            SDL_DestroyWindow(Window);
            SDL_Quit();
            TTF_Quit();
            IMG_Quit();
        }
    }
}