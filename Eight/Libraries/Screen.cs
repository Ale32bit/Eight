using KeraLua;

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
    }
}