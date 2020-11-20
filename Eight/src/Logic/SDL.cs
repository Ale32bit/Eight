using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static SDL2.SDL;

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

            Console.WriteLine("Creating renderer...");
            Renderer = SDL_CreateRenderer(Window, -1,
                SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (Renderer == IntPtr.Zero) {
                Console.WriteLine("SDL_CreateRenderer Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

            SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "0");
            SDL_SetRenderDrawBlendMode(Surface, SDL_BlendMode.SDL_BLENDMODE_NONE);

            return true;
        }

        public static void CreateCanvas() {
            if (Surface != IntPtr.Zero) {
                SDL_FreeSurface(Surface);
                Surface = IntPtr.Zero;
            }
            
            Surface = SDL_CreateRGBSurface(0, Eight.WindowWidth * Eight.WindowScale,
                Eight.WindowHeight * Eight.WindowScale, 32,
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
            IntPtr sTexture = SDL_CreateTextureFromSurface(Renderer, Surface);

            SDL_RenderClear(Renderer);

            SDL_RenderCopy(Renderer, sTexture, IntPtr.Zero, IntPtr.Zero);

            SDL_RenderPresent(Renderer);

            SDL_DestroyTexture(sTexture);
        }

        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            var pixel = new SDL_Rect {
                x = x * Eight.WindowScale,
                y = y * Eight.WindowScale,
                w = Eight.WindowScale,
                h = Eight.WindowScale
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(Surface);

            SDL_FillRect(Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));
        }
        
        public static void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            var rect = new SDL_Rect {
                x = x * Eight.WindowScale,
                y = y * Eight.WindowScale,
                w = w * Eight.WindowScale,
                h = h * Eight.WindowScale,
            };

            SDL_Surface sur = Marshal.PtrToStructure<SDL_Surface>(Surface);

            SDL_FillRect(Surface, ref rect, SDL_MapRGB(sur.format, r, g, b));
        }

        public static void Quit() {
            if (Surface != IntPtr.Zero)
                SDL_FreeSurface(Surface);
            
            SDL_DestroyRenderer(Renderer);
            SDL_DestroyWindow(Window);
        }
    }
}