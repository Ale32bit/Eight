using System;
using static SDL2.SDL;
using System.Drawing;
//using static SDL2.SDL_ttf;

namespace Eight {
    public class Display {
        public static IntPtr Window = IntPtr.Zero;
        public static IntPtr Renderer = IntPtr.Zero;
        public static IntPtr Surface = IntPtr.Zero;

        public static bool Dirty = true;

        public static ulong[] TextGrid;

        public static EBF TextFont;

        public static bool Init() {
            Console.WriteLine("Initializing SDL...");

            SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");

            if (SDL_Init(SDL_INIT_EVENTS | SDL_INIT_VIDEO | SDL_INIT_AUDIO) != 0) {
                Console.WriteLine("SDL_Init Error: {0}", SDL_GetError());
                SDL_Quit();
                return false;
            }

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

            var icon = LoadImage("../icon.png");

            SDL_SetWindowIcon(Window, icon);

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

            Console.WriteLine("Loading EBF font...");
            try {
                TextFont = new EBF("../Assets/font.ebf");
            } catch(System.IO.FileNotFoundException e) {
                Console.WriteLine("Could not find font.ebf");
                Console.WriteLine(e);
                return false;
            }

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
        }

        public static IntPtr BitmapToSurface(Bitmap bmp) {
            var r = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var cvt = bmp.Clone(r, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            bmp.Dispose();
            var dat = cvt.LockBits(r, System.Drawing.Imaging.ImageLockMode.ReadOnly, cvt.PixelFormat);
            var w = dat.Width;
            var h = dat.Height;
            var stride = dat.Stride;
            IntPtr surface = SDL_CreateRGBSurfaceWithFormat(0, w, h, 32, SDL_PIXELFORMAT_ARGB8888);
            unsafe { // hic sunt dracones
                var s = (SDL_Surface*)surface;
                var pitch = s->pitch;
                var dst = (byte*)s->pixels;
                var src = (byte*)dat.Scan0;
                for ( int y = 0; y < h; y++ ) {
                    Buffer.MemoryCopy(src, dst, w * 4, w * 4);
                    src += stride;
                    dst += pitch;
                }
            }
            cvt.UnlockBits(dat);
            cvt.Dispose();
            return surface;
        }

        public static IntPtr LoadImage(string path) {
            return BitmapToSurface(new(path));
        }

    }
}