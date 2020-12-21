using System;
using System.Runtime.InteropServices;
using Eight.Logic;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Eight.LuaLibs {
    public class ScreenShapes {

        public static void DrawRectangle(int x, int y, int w, int h, byte r, byte g, byte b) {
            if (Eight.IsQuit) return;

            var rect = new SDL_Rect {
                x = x,
                y = y,
                w = w,
                h = h
            };

            SDL_SetRenderDrawColor(SDL.Renderer, r, g, b, 255);
            SDL_RenderFillRect(SDL.Renderer, ref rect);

            /*var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref rect, SDL_MapRGB(sur.format, r, g, b));*/
        }

        public static void DrawRectangles(Utils.Coords[] coords, byte r, byte g, byte b) {
            if (Eight.IsQuit) return;

            SDL_Rect[] rects = new SDL_Rect[coords.Length];

            for (int i = 0; i < coords.Length; i++) {
                rects[i] = new SDL_Rect {
                    x = coords[i].x,
                    y = coords[i].y,
                    w = coords[i].w ?? 1,
                    h = coords[i].h ?? 1,
                };
            }

            SDL_SetRenderDrawColor(SDL.Renderer, r, g, b, 255);
            SDL_RenderFillRects(SDL.Renderer, rects, rects.Length);

            /*var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRects(SDL.Surface, rects, rects.Length, SDL_MapRGB(sur.format, r, g, b));*/
        }

        public static void DrawPixel(int x, int y, byte r, byte g, byte b) {
            if (Eight.IsQuit) return;

            if (x < 0 && y < 0 && x >= Eight.WindowWidth && y >= Eight.WindowHeight) {
                return;
            }

            SDL_SetRenderDrawColor(SDL.Renderer, r, g, b, 255);
            SDL_RenderDrawPoint(SDL.Renderer, x, y);

            /*var pixel = new SDL_Rect {
                x = x,
                y = y,
                w = 1,
                h = 1
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRect(SDL.Surface, ref pixel, SDL_MapRGB(sur.format, r, g, b));*/
        }

        public static void DrawPixels(Utils.Coords[] coords, byte r, byte g, byte b) {
            if (Eight.IsQuit) return;

            SDL_Point[] points = new SDL_Point[coords.Length];

            for (int i = 0; i < coords.Length; i++) {
                points[i] = new SDL_Point {
                    x = coords[i].x,
                    y = coords[i].y,
                };
            }

            SDL_SetRenderDrawColor(SDL.Renderer, r, g, b, 255);
            SDL_RenderDrawPoints(SDL.Renderer, points, points.Length);

            /*var sur = Marshal.PtrToStructure<SDL_Surface>(SDL.Surface);

            SDL_FillRects(SDL.Surface, rects, rects.Length, SDL_MapRGB(sur.format, r, g, b));*/
        }

        public static SDL_Surface DrawText(IntPtr font, string text, int x, int y, byte r, byte g, byte b) {
            var color = new SDL_Color {
                r = r,
                g = g,
                b = b
            };
            var textSurface = TTF_RenderUTF8_Solid(font, text, color);

            var tx = Marshal.PtrToStructure<SDL_Surface>(textSurface);

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