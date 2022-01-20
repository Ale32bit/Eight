using KeraLua;
using SDL2;

namespace Eight.Libraries
{
    class Screen : ILibrary
    {
        public string Name => "screen";
        public bool Global => false;


        public LuaRegister[] Registers => new LuaRegister[] {
            new() {
                name = "drawPixel",
                function = L_DrawPixel,
            },
            new() {
                name = "drawLine",
                function = L_DrawLine,
            },
            new(), // NULL
        };

        private static int L_DrawPixel(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var x = (int)state.CheckNumber(1);
            var y = (int)state.CheckNumber(2);
            var c = (int)state.CheckNumber(3);

            Program.Screen.SetPixel(x, y, (uint)c);
            return 0;
        }

        private static int L_DrawLine(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);

            var x1 = (int)state.CheckNumber(1);
            var y1 = (int)state.CheckNumber(2);
            var x2 = (int)state.CheckNumber(3);
            var y2 = (int)state.CheckNumber(4);
            long c = state.CheckInteger(5);

            byte a, r, g, b;
            b = (byte)((c) & 0xff);
            g = (byte)((c >> 8) & 0xff);
            r = (byte)((c >> 16) & 0xff);
            a = (byte)((c >> 24) & 0xff);
            SDL.SDL_SetRenderDrawColor(Program.Screen.Renderer, r, g, b, a);
            SDL.SDL_RenderDrawLine(Program.Screen.Renderer, x1, y1, x2, y2);

            return 0;
        }
    }
}