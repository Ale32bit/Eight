using System;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace Eight {
    public class SDLLogic {
        public static bool Init() {
            // Init SDL2 engine
            Console.WriteLine("Init SDL...");

            if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_EVENTS | SDL_INIT_AUDIO) != 0) {
                Console.WriteLine("SDL_Init Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            // Create Window
            Console.WriteLine("Creating Window...");

            Eight.Window = SDL_CreateWindow("Eight " + Eight.Version, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED,
                Eight.WindowWidth * Eight.WindowScale,
                Eight.WindowHeight * Eight.WindowScale,
                SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI);

            if (Eight.Window == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateWindow Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            // Create Renderer
            Console.WriteLine("Creating Renderer...");

            Eight.Renderer = SDL_CreateRenderer(Eight.Window, -1,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (Eight.Renderer == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateRenderer Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "1");
            
            return true;
        }

        public static void CreateCanvas() {
            if (Eight.Surface != IntPtr.Zero) {
                SDL_FreeSurface(Eight.Surface);
                Eight.Surface = IntPtr.Zero;
            }


            Eight.Surface = SDL_CreateRGBSurface(0, Eight.WindowWidth * Eight.WindowScale,
                Eight.WindowHeight * Eight.WindowScale, 32,
                0xff000000,
                0x00ff0000,
                0x0000ff00,
                0x000000ff);
            if (Eight.Surface == IntPtr.Zero) {
                SDL_Log("SDL_CreateRGBSurface() failed: " + SDL_GetError());
                Eight.Quit();
            }
        }

        public static void ResizeCanvas(int width, int height, int scale) {
            Eight.WindowWidth = width;
            Eight.WindowHeight = height;
            Eight.WindowScale = scale;
            
            CreateCanvas();
        }

        public static void DrawCanvas() {
            IntPtr sTexture = SDL_CreateTextureFromSurface(Eight.Renderer, Eight.Surface);

            SDL_RenderClear(Eight.Renderer);

            SDL_RenderCopy(Eight.Renderer, sTexture, IntPtr.Zero, IntPtr.Zero);

            SDL_RenderPresent(Eight.Renderer);

            SDL_DestroyTexture(sTexture);
        }

        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            var pixel = new SDL_Rect();
            pixel.x = x * Eight.WindowScale;
            pixel.y = y * Eight.WindowScale;
            pixel.w = Eight.WindowScale;
            pixel.h = Eight.WindowScale;

            SDL_Surface sur = Marshal.PtrToStructure<SDL_Surface>(Eight.Surface);

            SDL_FillRect(Eight.Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));
        }

        public static void Clear() {
            
        }

        public static void Clear(byte r, byte g, byte b) {
            
        }
    }
}