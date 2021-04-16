using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using static SDL2.SDL;

namespace Eight.Module {
    public class Graphics : IModule {
        public bool ThreadReady {
            get => false;
        }

        public static LuaRegister[] GraphicsLib = {
            new() {
                name = "drawRectangle",
                function = DrawRectangleL
            },
            new() {
                name = "drawRectangles",
                function = DrawRectanglesL,
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

        public void Init(Lua state) {
            state.RequireF("graphics", OpenLib, false);
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

            state.CheckType(1, LuaType.Table);
            state.CheckNumber(2);

            int c = (int)state.ToNumber(2);

            state.SetTop(1);

            int size = (int)state.Length(1);

            state.ArgumentCheck(size % 4 == 0, 1, "expected a table size of multiple of 4");

            List<SDL_Rect> pixels = new();

            for ( int i = 1; i <= size; i += 4 ) {
                state.GetInteger(1, i);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + i);
                var x = (int)state.ToNumber(-1);
                state.Pop(1);

                state.GetInteger(1, i + 1);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + (i + 1));
                var y = (int)state.ToNumber(-1);
                state.Pop(1);

                state.GetInteger(1, i + 2);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + (i + 2));
                var w = (int)state.ToNumber(-1);
                state.Pop(1);

                state.GetInteger(1, i + 3);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + (i + 3));
                var h = (int)state.ToNumber(-1);
                state.Pop(1);

                pixels.Add(new() {
                    x = x,
                    y = y,
                    w = w,
                    h = h,
                });
            }

            DrawRectangles(pixels.ToArray(), c);

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

            state.CheckType(1, LuaType.Table);
            state.CheckNumber(2);

            int c = (int)state.ToNumber(2);

            state.SetTop(1);

            int size = (int)state.Length(1);

            state.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

            List<SDL_Point> pixels = new();

            for ( int i = 1; i <= size; i += 2 ) {
                state.GetInteger(1, i);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + i);
                var x = (int)state.ToNumber(-1);
                state.Pop(1);

                state.GetInteger(1, i + 1);
                state.ArgumentCheck(state.IsNumber(-1), 1, "expected number at index " + (i + 1));
                var y = (int)state.ToNumber(-1);
                state.Pop(1);

                if ( x >= 0 && y >= 0 && x < Eight.RealWidth && y < Eight.RealHeight ) {
                    pixels.Add(new() {
                        x = x,
                        y = y,
                    });
                }
            }

            DrawPixels(pixels.ToArray(), c);

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

            // this does not need to simulate "out of bounds" when out of bounds :p
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

            Display.Dirty = true;
        }

        public static unsafe void DrawPixel(int x, int y, int c) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 || y < 0 || x >= Eight.RealWidth || y >= Eight.RealHeight ) return;

            uint p = ((uint)c << 8) | 0xff;

            var pitch = ((SDL_Surface*)Display.Surface)->pitch;
            ((uint*)((SDL_Surface*)Display.Surface)->pixels)[x + y * pitch / 4] = p;

            Display.Dirty = true;
        }

        public static unsafe void DrawPixels(SDL_Point[] points, int c) {
            if ( Eight.IsQuitting ) return;

            uint p = ((uint)c << 8) | 0xff;
            var pitch = ((SDL_Surface*)Display.Surface)->pitch;

            foreach ( var point in points ) {
                if ( point.x < 0 || point.y < 0 || point.x >= Eight.RealWidth || point.y >= Eight.RealHeight ) continue;
                ((uint*)((SDL_Surface*)Display.Surface)->pixels)[point.x + point.y * pitch / 4] = p;
            }

            Display.Dirty = true;
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

                if ( ch == ' ' ) {
                    dx += 4;
                }
            }

            DrawPixels(points.ToArray(), c);

            Display.Dirty = true;
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
            // out of bounds by 1 pixel to "simulate" the out of bounds ._.
            // the outline shouldn't be seen in the display border

            // is this even optimization? 
            if ( x < 0 ) x = -1;
            if ( y < 0 ) y = -1;
            if ( w > Eight.RealWidth ) w = Eight.RealWidth + 1;
            if ( h > Eight.RealHeight ) h = Eight.RealHeight + 1;

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

            Display.Dirty = true;
        }

        public static void DrawLine(int x1, int y1, int x2, int y2, int c) {
            if ( Eight.IsQuitting ) return;

            Color color = Color.FromArgb(c);
            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawLine(Display.Renderer, x1, y1, x2, y2);

            Display.Dirty = true;
        }

        public static void DrawLines(SDL_Point[] points, int c) {
            if ( Eight.IsQuitting ) return;

            Color color = Color.FromArgb(c);
            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderDrawLines(Display.Renderer, points, points.Length);

            Display.Dirty = true;
        }

        public static void DrawCircle(int x, int y, int r, int c) {
            if ( Eight.IsQuitting ) return;

            List<SDL_Point> points = new();

            for ( int i = 1; i <= 360; i++ ) {
                var angle = i * Math.PI / 180;
                var ptx = x * r * Math.Cos(angle);
                var pty = y * r * Math.Sin(angle);

                points.Add(new() {
                    x = (int)ptx,
                    y = (int)pty,
                });
            }

            DrawPixels(points.ToArray(), c);
        }
    }
}