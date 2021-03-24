using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static SDL2.SDL;

namespace Eight.Module {
    class ScreenText {
        public static int ForegroundColor = 0xffffff;
        public static int BackgroundColor = 0x000000;

        public static int SetChar(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckString(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.ArgumentCheck(state.IsNumber(4) || state.IsNoneOrNil(4), 4, "expected number, nil");

            var c = state.ToString(1);
            var x = (int)state.ToInteger(2);
            var y = (int)state.ToInteger(3);

            char ch;

            if ( c.Length > 0 ) {
                ch = c.ToCharArray()[0];
            } else {
                ch = ' ';
            }

            SetChar(ch, x, y, (!state.IsNoneOrNil(4) ? (Utils.TextFlag)state.ToInteger(4) : 0));

            return 0;
        }

        public static int SetForeground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);

            var c = state.ToInteger(1);

            c &= 0xffffff;

            var color = Color.FromArgb((int)c);

            ForegroundColor = color.ToArgb();

            return 0;
        }

        public static int SetBackground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);

            var c = state.ToInteger(1);

            c &= 0xffffff;

            var color = Color.FromArgb((int)c);

            BackgroundColor = color.ToArgb();

            return 0;
        }

        public static int GetForeground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushInteger(ForegroundColor);

            return 1;
        }

        public static int GetBackground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushInteger(BackgroundColor);

            return 1;
        }

        public static int Scroll(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            var n = state.ToInteger(1);

            if ( n == 0 ) return 0;
            if ( n <= -Eight.WindowHeight || n >= Eight.WindowHeight )
                return Clear(luaState);

            ulong[] newGrid = Enumerable.Repeat(Utils.ToULong(' ', ForegroundColor, BackgroundColor), Display.TextGrid.Length).ToArray();
            byte[] newFlags = new byte[Display.TextFlags.Length];

            long m = Math.Abs(n) * Eight.WindowWidth;
            if ( n < 0 ) { // text goes up, left shift
                Array.Copy(Display.TextGrid, m, newGrid, 0, Display.TextGrid.Length - m);
                Array.Copy(Display.TextFlags, m, newFlags, 0, Display.TextFlags.Length - m);
            } else { // text goes down, right shift
                Array.Copy(Display.TextGrid, 0, newGrid, m, Display.TextGrid.Length - m);
                Array.Copy(Display.TextFlags, 0, newFlags, m, Display.TextFlags.Length - m);
            }

            Display.TextGrid = newGrid;
            Display.TextFlags = newFlags;
            /*for (int i = 0; i < Display.TextGrid.Length; i++) {
                Display.TextGrid[i] = newGrid[i];
            }*/

            Redraw();
            Display.Dirty = true;

            return 0;
        }

        public static int Clear(IntPtr luaState) {
            ClearScreen();

            return 0;
        }

        public static void ClearScreen(bool resetGrid = true) {
            Color color = Color.FromArgb(BackgroundColor);
            SDL_SetRenderDrawColor(Display.Renderer, color.R, color.G, color.B, 255);
            SDL_RenderClear(Display.Renderer);

            if ( resetGrid )
                Display.TextGrid = Enumerable.Repeat(Utils.ToULong(' ', ForegroundColor, BackgroundColor), Display.TextGrid.Length).ToArray();
            Display.TextFlags = new byte[Display.TextFlags.Length];

            Display.Dirty = true;
        }

        public static void Redraw() {
            ClearScreen(false);
            for ( int y = 0; y < Eight.WindowHeight; y++ ) {
                for ( int x = 0; x < Eight.WindowWidth; x++ ) {
                    var point = Display.TextGrid[x + y * Eight.WindowWidth];
                    var tp = Utils.ToTextPoint(point);

                    DrawChar(tp.Char, x, y, tp.Foreground, tp.Background);
                }
            }
        }

        public static void RedrawChar(int x, int y) {
            var point = Display.TextGrid[x + y * Eight.WindowWidth];
            var tp = Utils.ToTextPoint(point);

            DrawChar(tp.Char, x, y, tp.Foreground, tp.Background);
        }
        public static void RedrawChar(int i) {
            var x = i % Eight.WindowWidth;
            var y = i / Eight.WindowWidth;
            RedrawChar(x, y);
        }

        public static void SetChar(char c, int x, int y, Utils.TextFlag flags = 0) {
            if ( x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight ) return;

            var point = Utils.ToULong(c, ForegroundColor, BackgroundColor);

            Display.TextGrid[x + y * Eight.WindowWidth] = point;
            Display.TextFlags[x + y * Eight.WindowWidth] = (byte)((byte)flags & ~(1 << 8));

            DrawChar(c, x, y, ForegroundColor, BackgroundColor);
        }

        public static int GetChar(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);

            var x = (int)state.ToInteger(1);
            var y = (int)state.ToInteger(2);

            if ( x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight ) return 0;

            var point = Utils.ToTextPoint(Display.TextGrid[x + y * Eight.WindowWidth]);
            var flags = Display.TextFlags[x + y * Eight.WindowWidth];

            state.PushString(point.Char.ToString());
            state.PushInteger(point.Foreground);
            state.PushInteger(point.Background);
            state.PushNumber(flags);

            return 4;
        }

        public static unsafe void DrawChar(char c, int x, int y, int fg, int bg) {
            if ( Eight.IsQuitting ) return;

            if ( x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight ) return;

            var flag = (Utils.TextFlag)Display.TextFlags[x + y * Eight.WindowWidth];

            if ( flag.HasFlag(Utils.TextFlag.Reversed) ) {
                var fgc = fg;
                fg = bg;
                bg = fgc;
            }

            if ( flag.HasFlag(Utils.TextFlag.Blinking) && Display.BlinkOn ) {
                var fgc = fg;
                fg = bg;
                bg = fgc;
            }

            if ( c >= Display.TextFont.CharList.Length ) c = '\uFFFD';
            var matrix = Display.TextFont.CharList[c];
            if ( matrix == null ) matrix = Display.TextFont.CharList['\uFFFD'];

            // Draw BG
            Graphics.DrawRectangle(x * Eight.CellWidth, y * Eight.CellHeight, Eight.CellWidth, Eight.CellHeight, bg);

            // Draw char
            List<SDL_Point> points = new();

            int deltaX = (Eight.CellWidth - matrix.GetLength(1)) / 2;
            for ( int gy = 0; gy < matrix.GetLength(0); gy++ ) {
                for ( int gx = 0; gx < matrix.GetLength(1); gx++ ) {
                    if ( matrix[gy, gx] ) {
                        points.Add(new() {
                            x = gx + (x * Eight.CellWidth) + deltaX,
                            y = gy + (y * Eight.CellHeight),
                        });
                    }
                }
            }

            Graphics.DrawPixels(points.ToArray(), fg);

            // Draw flags
            if ( flag.HasFlag(Utils.TextFlag.Underlined) ) {
                Graphics.DrawRectangle(x * Eight.CellWidth, y * Eight.CellHeight + Eight.CellHeight - 1, Eight.CellWidth, 1, fg);
            }

            if ( flag.HasFlag(Utils.TextFlag.Strikethrough) ) {
                Graphics.DrawRectangle(x * Eight.CellWidth, y * Eight.CellHeight + Eight.CellHeight / 2, Eight.CellWidth, 1, fg);
            }

            Display.Dirty = true;
        }

        public static void SetFlags(Utils.TextFlag flags, int x, int y) {
            Display.TextFlags[x + y * Eight.WindowWidth] = (byte)flags;
        }

        public static Utils.TextFlag GetFlags(int x, int y) {
            return (Utils.TextFlag)Display.TextFlags[x + y * Eight.WindowWidth];
        }
    }
}
