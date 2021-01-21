using System;
using System.Drawing;
using System.Numerics;
using static SDL2.SDL;

namespace Eight.Module {
    public class ScreenShapes {
        public static unsafe void DrawRectangle(int x, int y, int w, int h, int c) {
            if ( Eight.IsQuitting ) return;

            // Optimization?
            if ( x < 0 ) x = 0;
            if ( y < 0 ) y = 0;
            if ( w > Eight.RealWidth ) w = Eight.RealWidth;
            if ( h > Eight.RealHeight ) h = Eight.RealHeight;

            Color color = Color.FromArgb(c);

            var rect = new SDL_Rect {
                x = x,
                y = y,
                w = w,
                h = h
            };

            SDL_FillRect(Display.Surface, ref rect, SDL_MapRGB(((SDL_Surface*)Display.Surface)->format, color.R, color.G, color.B));

            Display.Dirty = true;
        }

        public static unsafe void DrawPixel(int x, int y, int c) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 && y < 0 && x >= Eight.RealWidth && y >= Eight.RealHeight ) {
                return;
            }

            Color color = Color.FromArgb(c);

            var pixel = new SDL_Rect {
                x = x,
                y = y,
                w = 1,
                h = 1
            };

            SDL_FillRect(Display.Surface, ref pixel, SDL_MapRGB(((SDL_Surface*)Display.Surface)->format, color.R, color.G, color.B));

            Display.Dirty = true;
        }

        public static void DrawString(string text, int x, int y, int c, int spacing) {
            var chars = text.ToCharArray();

            var dx = x;

            for ( int i = 0; i < chars.Length; i++ ) {
                var ch = chars[i];
                
                if ( ch >= Display.TextFont.CharList.Length ) ch = '?';
                var matrix = Display.TextFont.CharList[ch];
                if ( matrix == null ) matrix = Display.TextFont.CharList['?'];

                for ( int gy = 0; gy < matrix.GetLength(0); gy++ ) {
                    for ( int gx = 0; gx < matrix.GetLength(1); gx++ ) {
                        if ( matrix[gy, gx] ) {
                            DrawPixel(gx + dx, gy + y, c);
                        }
                    }
                }
                dx += matrix.GetLength(1) + spacing;

                if(ch == ' ') {
                    dx += 4;
                }
            }
        }

        public static Vector2 GetStringBounds(string text, int spacing) {
            var chars = text.ToCharArray();

            var w = 0;
            var h = 0;

            for ( int i = 0; i < chars.Length; i++ ) {
                var ch = chars[i];

                if ( ch >= Display.TextFont.CharList.Length ) ch = '?';
                var matrix = Display.TextFont.CharList[ch];
                if ( matrix == null ) matrix = Display.TextFont.CharList['?'];

                w += matrix.GetLength(1) + spacing;
                h = Math.Max(h, matrix.GetLength(0));

                if ( ch == ' ' ) {
                    w += 4;
                }
            }

            return new Vector2(w - spacing, h);
        }
    }
}