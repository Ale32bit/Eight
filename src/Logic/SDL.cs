using System;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Eight.Logic {
    public class SDL {
        public static IntPtr Window = IntPtr.Zero;
        public static IntPtr Renderer = IntPtr.Zero;
        public static IntPtr Surface = IntPtr.Zero;

        public static bool Init() {
            Console.WriteLine("Initializing SDL...");

            SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (SDL_Init(SDL_INIT_EVENTS | SDL_INIT_VIDEO | SDL_INIT_AUDIO) != 0) {
                Console.WriteLine("SDL_Init Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            TTF_Init();

            ResetSize();

            Console.WriteLine("Creating window...");
            Window = SDL_CreateWindow("Eight " + Eight.Version, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                Eight.WindowWidth * Eight.WindowScale,
                Eight.WindowHeight * Eight.WindowScale,
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

            if (Window == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateWindow Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            var icon = SDL2.SDL_image.IMG_Load("../icon.png");
            if (icon != IntPtr.Zero) {
                SDL_SetWindowIcon(Window, icon);
            } else {
                Console.WriteLine("Failed loading icon.png");
            }

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "0");
            SDL_SetRenderDrawBlendMode(Surface, SDL_BlendMode.SDL_BLENDMODE_NONE);

            Console.WriteLine("Creating renderer...");
            Renderer = SDL_CreateRenderer(Window, -1,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (Renderer == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateRenderer Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            return true;
        }

        public static void CreateCanvas() {
            if (Surface != IntPtr.Zero) {
                SDL_FreeSurface(Surface); 
                Surface = IntPtr.Zero;
            }

            Surface = SDL_CreateRGBSurface(0, Eight.WindowWidth,
                Eight.WindowHeight, 32,
                0xff000000,
                0x00ff0000,
                0x0000ff00,
                0x000000ff);
            if (Surface != IntPtr.Zero) return;
            Console.WriteLine("SDL_CreateRGBSurface() failed: " + SDL_GetError());
            Eight.Quit();
        }

        private static void UpdateWindow() {
            SDL_SetWindowSize(Window,
                Eight.WindowWidth * Eight.WindowScale,
                Eight.WindowHeight * Eight.WindowScale
            );

            SDL_SetWindowPosition(Window, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED);

            CreateCanvas();
        }


        public static void SetSize(int width, int height, int scale) {
            Eight.WindowWidth = width;
            Eight.WindowHeight = height;
            Eight.WindowScale = scale;

            UpdateWindow();
        }

        public static void ResetSize() {
            Eight.WindowWidth = Eight.DefaultWidth;
            Eight.WindowHeight = Eight.DefaultHeight;
            Eight.WindowScale = Eight.DefaultScale;

            UpdateWindow();
        }

        public static void DrawCanvas() {
            var sTexture = SDL_CreateTextureFromSurface(Renderer, Surface);

            SDL_RenderClear(Renderer);

            SDL_RenderCopy(Renderer, sTexture, IntPtr.Zero, IntPtr.Zero);

            SDL_RenderPresent(Renderer);

            SDL_DestroyTexture(sTexture);
        }

        public static void Quit() {
            if (Surface != IntPtr.Zero)
                SDL_FreeSurface(Surface);

            SDL_DestroyRenderer(Renderer);
            SDL_DestroyWindow(Window);
            SDL_Quit();
            TTF_Quit();
        }
    }
}