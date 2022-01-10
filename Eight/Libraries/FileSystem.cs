using KeraLua;

namespace Eight.Libraries
{
    class FileSystem : ILibrary
    {
        public string Name => "fs";
        public bool Global => false;


        private static LuaRegister[] Registers = new LuaRegister[]
        {
            new()
            {
               name = "test",
               function = L_Test,
            },
            new(), // NULL
        };

        public static int L_Test(IntPtr state)
        {
            return 0;
        }
    }
}
