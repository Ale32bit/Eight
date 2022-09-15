using KeraLua;

namespace Eight.Libraries
{
    public interface ILibrary
    {
        public string Name { get; }
        public bool Global { get; }
        /// <summary>
        /// Registers contain the functions to expose to Lua as a module. The array must end with a null value.
        /// </summary>
        public LuaRegister[] Registers { get; }

        public Task PreInitAsync();
        public Task InitAsync();
        
        public virtual void Register(Lua state)
        {
            state.RequireF(Name, OpenLibrary, Global);
        }

        public virtual int OpenLibrary(IntPtr luaState)
        {
            var state = Lua.FromIntPtr(luaState);
            state.NewLib(Registers);
            return 1;
        }
    }
}