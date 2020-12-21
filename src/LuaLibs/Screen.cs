using System;
using System.Collections;
using System.Collections.Generic;
using KeraLua;
using SDL2;
using static SDL2.SDL;
using Lua = Eight.Logic.Lua;
using SDL = Eight.Logic.SDL;

namespace Eight.LuaLibs {
    public class Screen {
        public static LuaRegister[] Screen_lib = {
            new() {
                name = "setPixel",
                function = SetPixel
            },
            new() {
                name = "setPixels",
                function = SetPixels,
            },
            new() {
                name = "setSize",
                function = SetSize
            },
            new() {
                name = "getSize",
                function = GetSize
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
                function = Clear
            },
            new() {
                name = "drawRectangle",
                function = DrawRectangle
            },
            new() {
                name = "drawRectangles",

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
                name = "loadFont",
                function = LoadFont
            },
            new() {
                name = "drawText",
                function = DrawText
            },
            new()
        };

        public static void Setup() {
            Lua.LuaState.RequireF("screen", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.NewLib(Screen_lib);

            return 1;
        }

        public static int SetPixel(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");
            state.ArgumentCheck(state.IsNumber(2), 2, "expected number");
            state.ArgumentCheck(state.IsNumber(3), 3, "expected number");
            state.ArgumentCheck(state.IsNumber(4), 4, "expected number");
            state.ArgumentCheck(state.IsNumber(5), 5, "expected number");

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var r = (byte)state.ToNumber(3);
            var g = (byte)state.ToNumber(4);
            var b = (byte)state.ToNumber(5);

            ScreenShapes.DrawPixel(x, y, r, g, b);
            return 0;
        }

        public static int SetPixels(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsTable(1), 1, "expected table");
            state.ArgumentCheck(state.IsNumber(2), 2, "expected number");
            state.ArgumentCheck(state.IsNumber(3), 3, "expected number");
            state.ArgumentCheck(state.IsNumber(4), 4, "expected number");

            var r = (byte)state.ToNumber(2);
            var g = (byte)state.ToNumber(3);
            var b = (byte)state.ToNumber(4);

            int len = state.RawLen(-1);
            List<int> rawCoords = new();

            List<Utils.Coords> coords = new();

            int index = 1;

            if (state.IsTable(1)) {
                state.PushNil();

                // This is needed for it to even get the first value

                while (state.Next(index)) {
                    int coord = (int) state.ToInteger(-1);
                    state.Pop(1);

                    rawCoords.Add(coord);
                }
            }

            for (int i = 0; i < rawCoords.Count; i += 2) {
                int x = rawCoords[i];
                if (i+1 < rawCoords.Count) {
                    int y = rawCoords[i + 1];

                    coords.Add(new() {
                        x = x,
                        y = y,
                    });
                }
            }

            ScreenShapes.DrawPixels(coords.ToArray(), r, g, b);

            return 0;
        }

        public static int SetSize(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");
            state.ArgumentCheck(state.IsNumber(2), 2, "expected number");
            state.ArgumentCheck(state.IsNumber(3), 3, "expected number");

            var w = (int)state.ToNumber(1);
            var h = (int)state.ToNumber(2);
            var s = (int)state.ToNumber(3);

            SDL.SetSize(w, h, s);

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

        public static int GetSize(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.PushNumber(Eight.WindowWidth);
            state.PushNumber(Eight.WindowHeight);
            state.PushNumber(Eight.WindowScale);

            return 3;
        }

        public static int SetTickrate(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");

            var tickrate = (int)state.ToNumber(1);
            Eight.SetTickrate(tickrate);
            return 0;
        }

        public static int GetTickrate(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.PushInteger(Eight.Tickrate);

            return 1;
        }

        public static int Clear(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);
            SDL.CreateCanvas();
            return 0;
        }

        public static int DrawRectangle(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsNumber(1), 1, "expected number");
            state.ArgumentCheck(state.IsNumber(2), 2, "expected number");
            state.ArgumentCheck(state.IsNumber(3), 3, "expected number");
            state.ArgumentCheck(state.IsNumber(4), 4, "expected number");
            state.ArgumentCheck(state.IsNumber(5), 5, "expected number");
            state.ArgumentCheck(state.IsNumber(6), 6, "expected number");
            state.ArgumentCheck(state.IsNumber(7), 7, "expected number");

            var x = (int)state.ToNumber(1);
            var y = (int)state.ToNumber(2);
            var w = (int)state.ToNumber(3);
            var h = (int)state.ToNumber(4);
            var r = (byte)state.ToNumber(5);
            var g = (byte)state.ToNumber(6);
            var b = (byte)state.ToNumber(7);

            ScreenShapes.DrawRectangle(x, y, w, h, r, g, b);

            return 0;
        }

        public static int SetTitle(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");

            var title = state.ToString(1);

            SDL_SetWindowTitle(SDL.Window, title);

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
            var state = KeraLua.Lua.FromIntPtr(luaState);
            state.PushString(SDL_GetWindowTitle(SDL.Window));
            return 1;
        }

        public static int LoadFont(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            state.ArgumentCheck(state.IsString(1), 1, "expected string");
            state.ArgumentCheck(state.IsNumber(2), 2, "expected number");

            var path = state.ToString(1);
            var size = (int)state.ToInteger(2);

            var resolvedPath = FileSystem.Resolve(path);

            var font = ScreenShapes.LoadFont(resolvedPath, size);
            var height = SDL_ttf.TTF_FontHeight(font);

            state.NewTable();

            state.PushString("font");
            state.PushLightUserData(font);
            state.SetTable(-3);

            state.PushString("height");
            state.PushInteger(height);
            state.SetTable(-3);

            return 1;
        }

        public static int DrawText(IntPtr luaState) {
            var state = KeraLua.Lua.FromIntPtr(luaState);

            var font = state.ToUserData(1);
            var text = state.ToString(2);
            var x = (int)state.ToInteger(3);
            var y = (int)state.ToInteger(4);
            var r = (byte)state.ToInteger(5);
            var g = (byte)state.ToInteger(6);
            var b = (byte)state.ToInteger(7);

            var surface = ScreenShapes.DrawText(font, text, x, y, r, g, b);

            state.NewTable();

            state.PushString("width");
            state.PushInteger(surface.w);
            state.SetTable(-3);
            state.PushString("height");
            state.PushInteger(surface.h);
            state.SetTable(-3);

            return 1;
        }
    }
}