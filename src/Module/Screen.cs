using KeraLua;
using System;
using static SDL2.SDL;

namespace Eight.Module {
    public class Screen {
        public static LuaRegister[] ScreenLib = {
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

            Event.Push(ev);

            return 0;
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

            Event.Push(ev);

            return 0;
        }

        public static int GetTitle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.PushString(SDL_GetWindowTitle(Display.Window));
            return 1;
        }
    }
}