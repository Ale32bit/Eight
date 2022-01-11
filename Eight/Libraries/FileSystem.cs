using KeraLua;

namespace Eight.Libraries
{
    class FileSystem : ILibrary
    {
        public string Name => "fs";
        public bool Global => false;


        public LuaRegister[] Registers => new LuaRegister[]
        {
            new()
            {
               name = "test",
               function = L_Test,
            },
            new(), // NULL
        };

        public static int L_Test(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);
            state.PushString("Test is successful");
            return 1;
        }
    }
}
