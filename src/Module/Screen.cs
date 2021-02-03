using System;
using System.Collections.Generic;
using System.Numerics;
using KeraLua;
using static SDL2.SDL;

namespace Eight.Module {
    public class Screen {
        public static LuaRegister[] ScreenLib = {
            new() {
                name = "drawRectangle",
                function = DrawRectangle
            },
            new() {
                name = "drawPixel",
                function = DrawPixel
            },
            new() {
                name = "drawPixels",
                function = DrawPixels,
            },
            new() {
                name = "drawString",
                function = DrawString,
            },
            new() {
                name = "getStringBounds",
                function = GetStringBounds,
            },
            new() {
                name = "setSize",
                function = SetSize
            },
            new() {
                name = "getSize",
                function = GetSize,
            },
            new() {
                name = "getRealSize",
                function = GetRealSize
            },
            new() {
                name = "setTickrate",
                function = SetTickrate
            },
            new() {
                name = "getTickrate",
                function = GetTickrate
            },
            new() {
                name = "clear",
                function = ScreenText.Clear
            },
            new() {
                name = "setTitle",
                function = SetTitle
            },
            new() {
                name = "getTitle",
                function = GetTitle
            },
            new() {
                name = "setChar",
                function = ScreenText.SetChar,
            },
            new() {
                name = "getChar",
                function = ScreenText.GetChar,
            },
            new() {
                name = "setForeground",
                function = ScreenText.SetForeground,
            },
            new() {
                name = "setBackground",
                function = ScreenText.SetBackground,
            },
            new() {
                name = "getForeground",
                function = ScreenText.GetForeground,
            },
            new() {
                name = "getBackground",
                function = ScreenText.GetBackground,
            },
            new() {
                name = "scroll",
                function = ScreenText.Scroll,
            },
            new()
        };

        public static void Setup() {
            Runtime.LuaState.RequireF("screen", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(ScreenLib);

            return 1;
        }

        public static int SetSize(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);

            var w = (int)state.ToNumber(1);
            var h = (int)state.ToNumber(2);
            var s = (int)state.ToNumber(3);

            Display.SetScreenSize(w, h, s);

            Utils.LuaParameter[] ev = {
                new() {
                    Type = LuaType.String,
                    Value = "screen_resize"
                },
                new() {
                    Type = LuaType.Number,
                    Value = w,
                },
                new() {
                    Type = LuaType.Number,
                    Value = h,
                },
                new() {
                    Type = LuaType.Number,
                    Value = s,
                }
            };

            Eight.PushEvent(ev);

            return 0;
        }

        public static int DrawRectangle(IntPtr luaState) {
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

            ScreenShapes.DrawRectangle(x, y, w, h, c);

            return 0;
        }

        public static int DrawRectangles(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.Error("Work in progress");

            return 0;
        }

        public static int DrawPixel(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);
            state.CheckNumber(2);
            state.CheckNumber(3);

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var c = (int)state.ToInteger(3);

            ScreenShapes.DrawPixel(x, y, c);
            return 0;
        }

        public static int DrawPixels(IntPtr luaState) {
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

        public static int DrawString(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckString(1);
            state.CheckNumber(2);
            state.CheckNumber(3);
            state.CheckNumber(4);
            state.CheckNumber(5);

            string text = state.ToString(1);
            int x = (int)state.ToInteger(2);
            int y = (int)state.ToInteger(3);
            int c = (int)state.ToInteger(4);
            int? spacing = (int?)state.ToInteger(5);

            ScreenShapes.DrawString(text, x, y, c, spacing ?? 1);

            return 0;
        }

        private static int GetStringBounds(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsStringOrNumber(1), 1, "expected string");
            state.ArgumentCheck(state.IsNumber(2) || state.IsNoneOrNil(2), 2, "expected number, nil");

            string text = state.ToString(1);
            int? spacing = (int?)state.ToInteger(2);

            var bounds = ScreenShapes.GetStringBounds(text, spacing ?? 1);

            state.PushNumber(bounds.X);
            state.PushNumber(bounds.Y);

            return 2;
        }

        public static int GetSize(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushNumber(Eight.WindowWidth);
            state.PushNumber(Eight.WindowHeight);
            state.PushNumber(Eight.WindowScale);

            return 3;
        }

        public static int GetRealSize(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushNumber(Eight.RealWidth);
            state.PushNumber(Eight.RealHeight);
            state.PushNumber(Eight.WindowScale);

            return 3;
        }

        public static int SetTickrate(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckNumber(1);

            var tickrate = (int)state.ToNumber(1);
            Eight.SetTickrate(tickrate);
            return 0;
        }

        public static int GetTickrate(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushInteger(Eight.Tickrate);

            return 1;
        }

        public static int SetTitle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.CheckString(1);

            var title = state.ToString(1);

            SDL_SetWindowTitle(Display.Window, title);

            Utils.LuaParameter[] ev = {
                new() {
                    Type = LuaType.String,
                    Value = "screen_title"
                },
                new() {
                    Type = LuaType.String,
                    Value = title,
                }
            };

            Eight.PushEvent(ev);

            return 0;
        }

        public static int GetTitle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.PushString(SDL_GetWindowTitle(Display.Window));
            return 1;
        }
    }
}