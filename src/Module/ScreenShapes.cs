using System;
using System.Runtime.InteropServices;
using static SDL2.SDL;
//using static SDL2.SDL_ttf;

namespace Eight.Module {
    public class ScreenShapes {
        public static void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            if ( Eight.IsQuitting ) return;

            var rect = new SDL_Rect {
                x = x,
                y = y,
                w = w,
                h = h
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(Display.Surface);

            SDL_FillRect(Display.Surface, ref rect, SDL_MapRGB(sur.format, r, g, b));

            Display.Dirty = true;
        }

        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 && y < 0 && x >= Eight.RealWidth && y >= Eight.RealHeight ) {
                return;
            }

            var pixel = new SDL_Rect {
                x = x,
                y = y,
                w = 1,
                h = 1
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(Display.Surface);

            SDL_FillRect(Display.Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));

            Display.Dirty = true;
        }
    }
}