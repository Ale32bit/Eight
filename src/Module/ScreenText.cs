using KeraLua;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using static SDL2.SDL;
//using static SDL2.SDL_ttf;

namespace Eight.Module {
    class ScreenText {
        public static int ForegroundColor = 0xffffff;
        public static int BackgroundColor = 0x000000;

        public static int SetChar(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected char");
            state.ArgumentCheck(state.IsInteger(2) || state.IsNumber(2), 2, "expected integer");
            state.ArgumentCheck(state.IsInteger(3) || state.IsNumber(3), 3, "expected integer");

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

            state.ArgumentCheck(state.IsInteger(1) || state.IsNumber(1), 1, "expected integer");

            var c = state.ToInteger(1);

            c &= 0xffffff;

            var color = Color.FromArgb((int)c);

            ForegroundColor = color.ToArgb();

            return 0;
        }

        public static int SetBackground(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsInteger(1) || state.IsNumber(1), 1, "expected integer");

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

            state.ArgumentCheck(state.IsInteger(1), 1, "expected integer");
            var n = state.ToInteger(1);

            if (n == 0) return 0;
            if (n <= -Eight.WindowHeight || n >= Eight.WindowHeight)
                return Clear(luaState);

            ulong[] newGrid = new ulong[Display.TextGrid.Length];

            long m = Math.Abs(n) * Eight.WindowWidth;
            if (n < 0) { // text goes up, left shift
                Array.Copy(Display.TextGrid, m, newGrid, 0, Display.TextGrid.Length - m);
            } else { // text goes down, right shift
                Array.Copy(Display.TextGrid, 0, newGrid, m, Display.TextGrid.Length - m);
            }

            Display.TextGrid = newGrid;
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
            Color bg = Color.FromArgb(BackgroundColor);

            ScreenShapes.DrawRectangle(0, 0, Eight.RealWidth, Eight.RealHeight, bg.R, bg.G, bg.B);

            if (resetGrid)
                Display.TextGrid = new ulong[Display.TextGrid.Length];

            Display.Dirty = true;
        }

        public static void Redraw() {
            ClearScreen(false);
            for (int y = 0; y < Eight.WindowHeight; y++) {
                for (int x = 0; x < Eight.WindowWidth; x++) {
                    var point = Display.TextGrid[x + y * Eight.WindowWidth];
                    var tp = Utils.ToTextPoint(point);

                    DrawChar(tp.Char, x, y, tp.Foreground, tp.Background);
                }
            }
        }

        public static void SetChar(char c, int x, int y) {
            if (x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight) return;

            var point = Utils.ToULong(c, ForegroundColor, BackgroundColor);

            Display.TextGrid[x + y * Eight.WindowWidth] = point;

            DrawChar(c, x, y, ForegroundColor, BackgroundColor);
        }

        public static int GetChar(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected integer"); // because integers still fail 
            state.ArgumentCheck(state.IsNumber(2), 2, "expected integer");

            var x = (int)state.ToInteger(1);
            var y = (int)state.ToInteger(2);

            if (x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight) return 0;

            var point = Utils.ToTextPoint(Display.TextGrid[x + y * Eight.WindowWidth]);

            state.PushString(point.Char.ToString());
            state.PushInteger(point.Foreground);
            state.PushInteger(point.Background);

            return 3;
        }


        public static void DrawChar(char c, int x, int y, int fg, int bg) {
            if (Eight.IsQuitting) return;

            if (x < 0 || y < 0 || x >= Eight.WindowWidth || y >= Eight.WindowHeight) return;

            Color fgc = Color.FromArgb(fg);

            Color bgc = Color.FromArgb(bg);

            if ( c >= Display.TextFont.CharList.Length ) c = '?';
            var matrix = Display.TextFont.CharList[c];
            if ( matrix == null ) matrix = Display.TextFont.CharList['?'];
            
            var bgRectangle = new SDL_Rect {
                x = (x * Eight.CellWidth),
                y = (y * Eight.CellHeight),
                w = Eight.CellWidth,
                h = Eight.CellHeight,
            };

            var sur = Marshal.PtrToStructure<SDL_Surface>(Display.Surface);

            SDL_FillRect(Display.Surface, ref bgRectangle, SDL_MapRGB(sur.format, bgc.R, bgc.G, bgc.B));

            int deltaX = (Eight.CellWidth - matrix.GetLength(1)) / 2;
            for (int gy = 0; gy < matrix.GetLength(0); gy++ ) {
                for(int gx = 0; gx < matrix.GetLength(1); gx++) {
                    if ( matrix[gy, gx] ) {
                        ScreenShapes.DrawPixel(gx + (x * Eight.CellWidth) + deltaX, gy + (y * Eight.CellHeight), fgc.R, fgc.G, fgc.B);
                    }
                }
            }

            Display.Dirty = true;
        }

    }
}
