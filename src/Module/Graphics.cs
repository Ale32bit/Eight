using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using static SDL2.SDL;

namespace Eight.Module {
    public class Graphics {

        public static LuaRegister[] GraphicsLib = {
            new() {
                name = "drawRectangle",
                function = DrawRectangleL
            },
            new() {
                name = "drawPixel",
                function = DrawPixelL
            },
            new() {
                name = "drawPixels",
                function = DrawPixelsL,
            },
            new() {
                name = "drawString",
                function = DrawStringL,
            },
            new() {
                name = "getStringBounds",
                function = GetStringBoundsL,
            },
            new() {
                name = "drawOutline",
                function = DrawOutlineL,
            },
            new() {
                name = "drawLine",
                function = DrawLineL,
            },
            new(),
        };

        public static void Setup() {
            Runtime.LuaState.RequireF("graphics", OpenLib, false);
        }

        public static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(GraphicsLib);
            return 1;
        }

        // Lua functions, note the final L in the function names

        public static int DrawRectangleL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.CheckNumber(4);
            state.CheckNumber(5);

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var w = (int)state.ToNumber(3);
            var h = (int)state.ToNumber(4);
            var c = (int)state.ToNumber(5);

            DrawRectangle(x, y, w, h, c);

            return 0;
        }

        public static int DrawRectanglesL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.Error("Work in progress");

            return 0;
        }

        public static int DrawPixelL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var c = (int)state.ToInteger(3);

            DrawPixel(x, y, c);
            return 0;
        }

        public static int DrawPixelsL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.Error("Work in progress");



            /*state.CheckType(1, LuaType.Table);
            state.CheckNumber(2);

            int c = (int)state.ToNumber(2);

            state.SetTop(1);

            int size = (int)state.Length(1);

            state.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

            List<Vector2> pixels = new();

            for ( int i = 1; i <= size; i += 2 ) {
                // Get X
                state.PushInteger(i);
                state.GetTable(1);
                if ( !state.IsNumber(-1) ) {
                    state.Error("item %d invalid (number expected, got %s)", i, state.TypeName(-1));
                    return 0;
                }
                int x = (int)state.ToNumber(-1);
                state.Pop(-1);

                // Get Y
                state.PushInteger(i + 1);
                state.GetTable(1);
                if ( !state.IsNumber(-1) ) {
                    state.Error("item %d invalid (number expected, got %s)", i, state.TypeName(-1));
                    return 0;
                }

                int y = (int)state.ToNumber(-1);
                state.Pop(-1);

                // Add pixel to list
                pixels.Add(new(x, y));
            }

            ScreenShapes.DrawPixels(pixels.ToArray(), c);*/

            return 0;
        }

        public static int DrawStringL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckString(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.CheckNumber(4);
            state.ArgumentCheck(state.IsNoneOrNil(5) || state.IsNumber(5), 5, "expected number, nil");

            string text = state.ToString(1);
            int x = (int)state.ToInteger(2);
            int y = (int)state.ToInteger(3);
            int c = (int)state.ToInteger(4);
            int? spacing = (int?)state.ToInteger(5);

            DrawString(text, x, y, c, spacing ?? 1);

            return 0;
        }

        public static int GetStringBoundsL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsStringOrNumber(1), 1, "expected string");
            state.ArgumentCheck(state.IsNumber(2) || state.IsNoneOrNil(2), 2, "expected number, nil");

            string text = state.ToString(1);
            int? spacing = (int?)state.ToInteger(2);

            var bounds = GetStringBounds(text, spacing ?? 1);

            state.PushNumber(bounds.X);
            state.PushNumber(bounds.Y);

            return 2;
        }

        public static int DrawOutlineL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.CheckNumber(4);
            state.CheckNumber(5);

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var w = (int)state.ToNumber(3);
            var h = (int)state.ToNumber(4);
            var c = (int)state.ToNumber(5);

            DrawOutline(x, y, w, h, c);

            return 0;
        }

        public static int DrawLineL(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.CheckNumber(4);
            state.CheckNumber(5);

            var x1 = (int)state.ToNumber(1);
            var y1 = (int)state.ToNumber(2);
            var x2 = (int)state.ToNumber(3);
            var y2 = (int)state.ToNumber(4);
            var c = (int)state.ToNumber(5);

            DrawLine(x1, y1, x2, y2, c);

            return 0;
        }

        // Eight functions

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


            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderFillRect(Display.Renderer, ref rect);

            Display.Dirty = true;
        }

        public static unsafe void DrawRectangles(SDL_Rect[] rects, int c) {
            if ( Eight.IsQuitting ) return;

            Color color = Color.FromArgb(c);

            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderFillRects(Display.Renderer, rects, rects.Length);

        }

        public static unsafe void DrawPixel(int x, int y, int c) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 && y < 0 && x >= Eight.RealWidth && y >= Eight.RealHeight ) return;

            Color color = Color.FromArgb(c);

            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawPoint(Display.Renderer, x, y);

            Display.Dirty = true;
        }

        public static unsafe void DrawPixels(SDL_Point[] points, int c) {
            if ( Eight.IsQuitting ) return;

            Color color = Color.FromArgb(c);

            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawPoints(Display.Renderer, points, points.Length);
        }

        public static void DrawString(string text, int x, int y, int c, int spacing) {
            var chars = text.ToCharArray();

            var dx = x;

            List<SDL_Point> points = new();

            for ( int i = 0; i < chars.Length; i++ ) {
                var ch = chars[i];
                
                if ( ch >= Display.TextFont.CharList.Length ) ch = '\uFFFD';
                var matrix = Display.TextFont.CharList[ch];
                if ( matrix == null ) matrix = Display.TextFont.CharList['\uFFFD'];

                for ( int gy = 0; gy < matrix.GetLength(0); gy++ ) {
                    for ( int gx = 0; gx < matrix.GetLength(1); gx++ ) {
                        if ( matrix[gy, gx] ) {
                            points.Add(new() {
                                x = gx + dx,
                                y = gy + y,
                            });
                        }
                    }
                }

                dx += matrix.GetLength(1) + spacing;

                if(ch == ' ') {
                    dx += 4;
                }
            }

            DrawPixels(points.ToArray(), c);
        }

        public static Vector2 GetStringBounds(string text, int spacing) {
            var chars = text.ToCharArray();

            var w = 0;
            var h = 0;

            for ( int i = 0; i < chars.Length; i++ ) {
                var ch = chars[i];

                if ( ch >= Display.TextFont.CharList.Length ) ch = '\uFFFD';
                var matrix = Display.TextFont.CharList[ch];
                if ( matrix == null ) matrix = Display.TextFont.CharList['\uFFFD'];

                w += matrix.GetLength(1) + spacing;
                h = Math.Max(h, matrix.GetLength(0));

                if ( ch == ' ' ) {
                    w += 4;
                }
            }

            return new Vector2(w - spacing, h);
        }

        // New graphics functions

        public static void DrawOutline(int x, int y, int w, int h, int c) {
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


            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawRect(Display.Renderer, ref rect);

            Display.Dirty = true;
        }

        public static void DrawOutlines(SDL_Rect[] rects, int c) {
            if ( Eight.IsQuitting ) return;

            Color color = Color.FromArgb(c);

            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawRects(Display.Renderer, rects, rects.Length);

        }

        public static void DrawLine(int x1, int y1, int x2, int y2, int c) {
            Color color = Color.FromArgb(c);
            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawLine(Display.Renderer, x1, y1, x2, y2);
        }

        public static void DrawLines(SDL_Point[] points, int c) {
            Color color = Color.FromArgb(c);
            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawLines(Display.Renderer, points, points.Length);
        }
    }
}