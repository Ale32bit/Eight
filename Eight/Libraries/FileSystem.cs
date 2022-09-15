using KeraLua;

namespace Eight.Libraries
{
    class FileSystem : ILibrary
    {
        public string Name => "fs";
        public bool Global => false;


        public LuaRegister[] Registers => new LuaRegister[] {
            new() {
                name = "read",
                function = L_Read,
            },
            new(), // NULL
        };

        public async Task PreInitAsync()
        {
            
        }

        public async Task InitAsync()
        {
        }

        private static int L_Read(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);
            var path = state.CheckString(1);
            
            state.PushString(File.ReadAllText(path));
            
            return 1;
        }
    }
}