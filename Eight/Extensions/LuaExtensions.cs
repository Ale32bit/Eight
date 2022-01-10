using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eight.Extensions
{
    static class LuaExtensions
    {
        public static int OpenBase(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_base(lua.Handle);
        }

        public static int OpenCoroutine(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_coroutine(lua.Handle);
        }

        public static int OpenDebug(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_debug(lua.Handle);
        }

        public static int OpenIO(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_io(lua.Handle);
        }

        public static int OpenMath(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_math(lua.Handle);
        }

        public static int OpenOS(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_os(lua.Handle);
        }

        public static int OpenPackage(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_package(lua.Handle);
        }

        public static int OpenString(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_string(lua.Handle);
        }

        public static int OpenTable(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_table(lua.Handle);
        }

        public static int OpenUTF8(this Lua lua)
        {
            return NativeLuaExtensions.luaopen_utf8(lua.Handle);
        }
    }
}
