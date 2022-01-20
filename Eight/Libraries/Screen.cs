using KeraLua;
using static SDL2.SDL;

namespace Eight.Libraries
{
    class Screen : ILibrary
    {
        public string Name => "screen";
        public bool Global => false;


        public LuaRegister[] Registers => new LuaRegister[] {
            new()
            {
                name = "getSize",
                function = L_GetSize,
            },

            // Drawing functions
            new() {
                name = "setPixel",
                function = L_SetPixel,
            },
            new()
            {
                name = "getPixel",
                function = L_GetPixel,
            },
            new()
            {
                name = "setColor",
                function= L_SetColor,
            },
            new()
            {
                name = "getColor",
                function= L_GetColor,
            },
            new() {
                name = "clear",
                function = L_Clear,
            },
            new() {
                name = "drawLine",
                function = L_DrawLine,
            },
            new(), // NULL
        };

        private static int L_GetSize(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            state.PushNumber(Program.Screen.RealWidth);
            state.PushNumber(Program.Screen.RealHeight);

            return 2;
        }

        // Drawing functions

        // SetPixel and GetPixel are unsafe because no out of bound checks
        private static int L_SetPixel(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var x = (int)state.CheckNumber(1);
            var y = (int)state.CheckNumber(2);
            var r = (byte)state.CheckNumber(3);
            var g = (byte)state.CheckNumber(4);
            var b = (byte)state.CheckNumber(5);

            if (x < 0 || y < 0 || x >= Program.Screen.RealWidth || y >= Program.Screen.RealHeight)
            {
                state.Error("Out of bounds");
                return 0;
            }

            uint c = 0xff; // Alpha
            c = c << 8 | r;
            c = c << 8 | g;
            c = c << 8 | b;

            Program.Screen.SetPixel(x, y, c);
            return 0;
        }

        private static int L_GetPixel(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var x = (int)state.CheckNumber(1);
            var y = (int)state.CheckNumber(2);

            if (x < 0 || y < 0 || x >= Program.Screen.RealWidth || y >= Program.Screen.RealHeight)
            {
                state.Error("Out of bounds");
                return 0;
            }

            var c = Program.Screen.GetPixel(x, y);

            state.PushNumber(c & 0xff0000);
            state.PushNumber(c & 0xff00);
            state.PushNumber(c & 0xff);

            return 3;
        }

        private static int L_SetColor(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var argsAmount = state.GetTop();

            byte r, g, b;

            if (argsAmount == 1)
            {
                var c = (int)state.CheckNumber(1);
                r = (byte)((c >> 16) & 0xff);
                g = (byte)((c >> 8) & 0xff);
                b = (byte)((c) & 0xff);
            }
            else if (argsAmount == 3)
            {
                r = (byte)state.CheckNumber(1);
                g = (byte)state.CheckNumber(2);
                b = (byte)state.CheckNumber(3);
            }
            else
            {
                return 0;
            }

            SDL_SetRenderDrawColor(Program.Screen.Renderer, 255, r, g, b);

            return 0;
        }

        private static int L_GetColor(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            if (SDL_GetRenderDrawColor(Program.Screen.Renderer, out var r, out var g, out var b, out _) != 0)
            {
                state.Error(SDL_GetError());
            }

            state.PushNumber(r);
            state.PushNumber(g);
            state.PushNumber(b);

            return 3;
        }

        private static int L_Clear(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            SDL_RenderClear(Program.Screen.Renderer);

            return 0;
        }

        private static int L_DrawLine(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var x1 = (int)state.CheckNumber(1);
            var y1 = (int)state.CheckNumber(2);
            var x2 = (int)state.CheckNumber(3);
            var y2 = (int)state.CheckNumber(4);

            SDL_RenderDrawLine(Program.Screen.Renderer, x1, y1, x2, y2);

            return 0;
        }
    }
}