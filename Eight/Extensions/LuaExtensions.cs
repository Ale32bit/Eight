using KeraLua;

namespace Eight.Extensions
{
    static class LuaExtensions
    {
        public static void OpenBase(this Lua lua)
        {
            lua.RequireF("_G", NativeLuaExtensions.luaopen_base, true);
        }

        public static void OpenCoroutine(this Lua lua)
        {
            lua.RequireF("coroutine", NativeLuaExtensions.luaopen_coroutine, true);
        }

        public static void OpenDebug(this Lua lua)
        {
            lua.RequireF("debug", NativeLuaExtensions.luaopen_debug, true);
        }

        public static void OpenIO(this Lua lua)
        {
            lua.RequireF("io", NativeLuaExtensions.luaopen_io, true);
        }

        public static void OpenMath(this Lua lua)
        {
            lua.RequireF("math", NativeLuaExtensions.luaopen_math, true);
        }

        public static void OpenOS(this Lua lua)
        {
            lua.RequireF("os", NativeLuaExtensions.luaopen_os, true);
        }

        public static void OpenPackage(this Lua lua)
        {
            lua.RequireF("package", NativeLuaExtensions.luaopen_package, true);
        }

        public static void OpenString(this Lua lua)
        {
            lua.RequireF("string", NativeLuaExtensions.luaopen_string, true);
        }

        public static void OpenTable(this Lua lua)
        {
            lua.RequireF("table", NativeLuaExtensions.luaopen_table, true);
        }

        public static void OpenUTF8(this Lua lua)
        {
            lua.RequireF("utf8", NativeLuaExtensions.luaopen_utf8, true);
        }
    }
}
