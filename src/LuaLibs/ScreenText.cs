using KeraLua;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Eight.LuaLibs {
    class ScreenText {
        public static int ForegroundColor = 0xffffff;
        public static int BackgroundColor = 0x000000;

        public static int SetChar(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected char");
            state.ArgumentCheck(state.IsInteger(2), 2, "expected integer");
            state.ArgumentCheck(state.IsInteger(3), 3, "expected integer");

            var c = state.ToString(1);
            var x = (int)state.ToInteger(2);
            var y = (int)state.ToInteger(3);

            char ch;

            if (c.Length > 0) {
                ch = c.ToCharArray()[0];
            } else {
                ch = ' ';
            }

            SetChar(ch, x, y);

            return 0;
        }

        public static int SetForeground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            var r = state.ToNumber(1);
            var g = state.ToNumber(2);
            var b = state.ToNumber(3);

            var color = Color.FromArgb(0, (int)r, (int)g, (int)b);

            ForegroundColor = color.ToArgb();

            return 0;
        }

        public static int SetBackground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            var r = state.ToNumber(1);
            var g = state.ToNumber(2);
            var b = state.ToNumber(3);

            var color = Color.FromArgb(0, (int)r, (int)g, (int)b);

            BackgroundColor = color.ToArgb();

            return 0;
        }

        public static int Scroll(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsInteger(1), 1, "integer expected");

            var n = state.ToInteger(1);

            if (n == 0) return 0;

            if (n <= -Eight.WindowHeight || n >= Eight.WindowHeight)
                return Clear(luaState);

            ulong[] newGrid = new ulong[Logic.SDL.TextGrid.Length];

            long m = Math.Abs(n) * Eight.WindowWidth;

            if (n < 0) { // text goes up, left shift
                Array.Copy(Logic.SDL.TextGrid, m, newGrid, 0, Logic.SDL.TextGrid.Length - m);
            } else { // text goes down, right shift
                Array.Copy(Logic.SDL.TextGrid, 0, newGrid, m, Logic.SDL.TextGrid.Length - m);
            }

            //Logic.SDL.TextGrid = newGrid;
            for(int i = 0; i < Logic.SDL.TextGrid.Length; i++) {
                Logic.SDL.TextGrid[i] = newGrid[i];
            }

            Redraw();

            Logic.SDL.Dirty = true;
            return 0;
        }

        public static int Clear(IntPtr luaState) {
            ClearScreen();

            return 0;
        }

        public static void ClearScreen(bool resetGrid = true) {
            Color bg = Color.FromArgb(BackgroundColor);

            ScreenShapes.DrawRectangle(0, 0, Eight.RealWidth, Eight.RealHeight, bg.R, bg.G, bg.B);

            if (resetGrid)
                Logic.SDL.TextGrid = new ulong[Logic.SDL.TextGrid.Length];

            Logic.SDL.Dirty = true;
        }

        public static void Redraw() {
            ClearScreen(false);
            for (int y = 0; y < Eight.WindowHeight; y++) {
                for (int x = 0; x < Eight.WindowWidth; x++) {
                    var point = Logic.SDL.TextGrid[x + y * Eight.WindowWidth];
                    var tp = Utils.ToTextPoint(point);

                    DrawChar(tp.Char, x, y, tp.Foreground, tp.Background);
                }
            }
        }

        public static void SetChar(char c, int x, int y) {
            if (x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight) return;

            var point = Utils.ToULong(c, ForegroundColor, BackgroundColor);

            Logic.SDL.TextGrid[x + y * Eight.WindowWidth] = point;

            DrawChar(c, x, y, ForegroundColor, BackgroundColor);
        }

        public static void DrawChar(char c, int x, int y, int fg, int bg) {
            if (Eight.IsQuitting) return;

            if (x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight) return;

            Color fgc = Color.FromArgb(fg);
            SDL_Color foreground = new() {
                r = fgc.R,
                g = fgc.G,
                b = fgc.B,
            };

            Color bgc = Color.FromArgb(bg);

            var textSurface = TTF_RenderUTF8_Solid(Logic.SDL.TextFont, c.ToString(), foreground);

            if (textSurface == IntPtr.Zero) {
                return;
            }

            var fontHeight = TTF_FontHeight(Logic.SDL.TextFont);

            var tx = Marshal.PtrToStructure<SDL_Surface>(textSurface);

            var textRectangle = new SDL_Rect {
                x = (x * Eight.CellWidth) + (int)Math.Floor((double)(Eight.CellWidth - tx.w) / 2),
                y = (y * Eight.CellHeight) - 1,
                w = tx.w,
                h = tx.h
            };

            var bgRectangle = new SDL_Rect {
                x = (x * Eight.CellWidth),
                y = (y * Eight.CellHeight),
                w = tx.w,
                h = tx.h
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(Logic.SDL.Surface);

            SDL_FillRect(Logic.SDL.Surface, ref bgRectangle, SDL_MapRGB(sur.format, bgc.R, bgc.G, bgc.B));

            SDL_BlitSurface(textSurface, IntPtr.Zero, Logic.SDL.Surface, ref textRectangle);

            SDL_FreeSurface(textSurface);

            Logic.SDL.Dirty = true;
        }

    }
}
