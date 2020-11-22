using System;
using System.Runtime.InteropServices;
using Eight.Logic;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Eight.LuaLibs {
    public class ScreenShapes {
        public static void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            var rect = new SDL_Rect {
                x = x * Eight.WindowScale,
                y = y * Eight.WindowScale,
                w = w * Eight.WindowScale,
                h = h * Eight.WindowScale,
            };

            SDL_Surface sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref rect, SDL_MapRGB(sur.format, r, g, b));
        }
        
        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            var pixel = new SDL_Rect {
                x = x * Eight.WindowScale,
                y = y * Eight.WindowScale,
                w = Eight.WindowScale,
                h = Eight.WindowScale
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));
        }

        public static IntPtr LoadFont(string path, int size) {
            IntPtr font = TTF_OpenFont(path, size);
            if (font == IntPtr.Zero) {
                throw new Exception(SDL_GetError());
            }
            return IntPtr.Zero;
        }
    }
}