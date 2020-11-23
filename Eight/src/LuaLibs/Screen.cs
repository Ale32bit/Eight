using System;
using KeraLua;
using SDL2;
using static SDL2.SDL;

namespace Eight.LuaLibs {
    public class Screen {
        public static LuaRegister[] Screen_lib = {
            new LuaRegister {
                name = "setPixel",
                function = SetPixel,
            },
            new LuaRegister {
                name = "setSize",
                function = SetSize,
            },
            new LuaRegister {
                name = "getSize",
                function = GetSize,
            },
            new LuaRegister {
                name = "setTickrate",
                function = SetTickrate,
            },
            new LuaRegister {
                name = "getTickrate",
                function = GetTickrate,
            },
            new LuaRegister {
                name = "clear",
                function = Clear,
            },
            new LuaRegister {
                name = "drawRectangle",
                function = DrawRectangle,
            },
            new LuaRegister {
                name = "setTitle",
                function = SetTitle,
            },
            new LuaRegister {
                name = "getTitle",
                function = GetTitle,
            },
            new LuaRegister {
                name = "loadFont",
                function = LoadFont,
            },
            new LuaRegister {
                name = "drawText",
                function = DrawText,
            },
            new LuaRegister(),
        };

        public static void Setup() {
            Logic.Lua.LuaState.RequireF("screen", OpenLib, false);
        }

        private static int OpenLib(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(Screen_lib);

            return 1;
        }

        public static int SetPixel(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            int x = (int) state.ToNumber(1);
            int y = (int) state.ToNumber(2);
            byte r = (byte) state.ToNumber(3);
            byte g = (byte) state.ToNumber(4);
            byte b = (byte) state.ToNumber(5);

            ScreenShapes.DrawPixel(x, y, r, g, b);
            return 0;
        }

        public static int SetSize(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            int w = (int) state.ToNumber(1);
            int h = (int) state.ToNumber(2);
            int s = (int) state.ToNumber(3);

            Logic.SDL.SetSize(w, h, s);

            return 0;
        }

        public static int GetSize(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushNumber(Eight.WindowWidth);
            state.PushNumber(Eight.WindowHeight);
            state.PushNumber(Eight.WindowScale);

            return 3;
        }

        public static int SetTickrate(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            int tickrate = (int) state.ToNumber(1);
            Eight.SetTickrate(tickrate);
            return 0;
        }

        public static int GetTickrate(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            state.PushInteger(Eight.Tickrate);

            return 1;
        }

        public static int Clear(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            Logic.SDL.CreateCanvas();
            return 0;
        }

        public static int DrawRectangle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            int x = (int) state.ToNumber(1);
            int y = (int) state.ToNumber(2);
            int w = (int) state.ToNumber(3);
            int h = (int) state.ToNumber(4);
            byte r = (byte) state.ToNumber(5);
            byte g = (byte) state.ToNumber(6);
            byte b = (byte) state.ToNumber(7);

            ScreenShapes.DrawRectangle(x, y, w, h, r, g, b);

            return 0;
        }

        public static int SetTitle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            string title = state.ToString(1);
            SDL_SetWindowTitle(Logic.SDL.Window, title);
            return 0;
        }

        public static int GetTitle(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);
            state.PushString(SDL_GetWindowTitle(Logic.SDL.Window));
            return 1;
        }

        public static int LoadFont(IntPtr luaState) {
            var state = Lua.FromIntPtr(luaState);

            var path = state.ToString(1);
            var size = (int) state.ToInteger(2);

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
            var state = Lua.FromIntPtr(luaState);

            var font = state.ToUserData(1);
            var text = state.ToString(2);
            var x = (int) state.ToInteger(3);
            var y = (int) state.ToInteger(4);
            var r = (byte) state.ToInteger(5);
            var g = (byte) state.ToInteger(6);
            var b = (byte) state.ToInteger(7);
            
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