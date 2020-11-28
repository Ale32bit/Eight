using System;
using System.Runtime.InteropServices;
using Eight.Logic;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Eight.LuaLibs {
    public class ScreenShapes {
        public static void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            var rect = new SDL_Rect {
                x = x,
                y = y,
                w = w,
                h = h
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref rect, SDL_MapRGB(sur.format, r, g, b));
        }

        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            var pixel = new SDL_Rect {
                x = x,
                y = y,
                w = 1,
                h = 1
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));
        }

        // why isn't this function drawing anything?!!
        public static SDL_Surface DrawText(IntPtr font, string text, int x, int y, byte r, byte g, byte b) {
            var color = new SDL_Color {
                r = r,
                g = g,
                b = b
            };
            var textSurface = TTF_RenderUTF8_Solid(font, text, color);

            var tx = Marshal.PtrToStructure<SDL_Surface>(textSurface);

            Console.WriteLine($"{x} {y} {tx.w} {tx.h}");

            var textRectangle = new SDL_Rect {
                x = x,
                y = y,
                w = tx.w,
                h = tx.h
            };

            SDL_BlitSurface(textSurface, IntPtr.Zero, SDL.Surface, ref textRectangle);

            SDL_FreeSurface(textSurface);

            return tx;
        }

        public static IntPtr LoadFont(string path, int size) {
            var font = TTF_OpenFont(path, size);
            if (font == IntPtr.Zero) throw new Exception(SDL_GetError());

            return font;
        }
    }
}