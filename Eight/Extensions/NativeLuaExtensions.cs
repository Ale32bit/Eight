using System.Runtime.InteropServices;

namespace Eight.Extensions
{
    internal static class NativeLuaExtensions
    {
#if __IOS__ || __TVOS__ || __WATCHOS__ || __MACCATALYST__
        private const string LuaLibraryName = "@rpath/liblua54.framework/liblua54";
#elif __ANDROID__
        private const string LuaLibraryName = "liblua54.so";
#elif __MACOS__
        private const string LuaLibraryName = "liblua54.dylib";
#elif WINDOWS_UWP
        private const string LuaLibraryName = "lua54.dll";
#else
        private const string LuaLibraryName = "lua54";
#endif

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_base(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_coroutine(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_debug(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_io(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_math(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_os(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_package(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_string(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_table(IntPtr luaState);

        [DllImport(LuaLibraryName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int luaopen_utf8(IntPtr luaState);
    }
}
