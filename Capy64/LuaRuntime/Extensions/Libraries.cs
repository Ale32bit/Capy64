using KeraLua;

namespace Capy64.LuaRuntime.Extensions
{
    static class LuaExtensions
    {
        public static void OpenBase(this Lua lua)
        {
            lua.RequireF("_G", NativeLibraries.luaopen_base, true);
        }

        public static void OpenCoroutine(this Lua lua)
        {
            lua.RequireF("coroutine", NativeLibraries.luaopen_coroutine, true);
        }

        public static void OpenDebug(this Lua lua)
        {
            lua.RequireF("debug", NativeLibraries.luaopen_debug, true);
        }

        public static void OpenIO(this Lua lua)
        {
            lua.RequireF("io", NativeLibraries.luaopen_io, true);
        }

        public static void OpenMath(this Lua lua)
        {
            lua.RequireF("math", NativeLibraries.luaopen_math, true);
        }

        public static void OpenOS(this Lua lua)
        {
            lua.RequireF("os", NativeLibraries.luaopen_os, true);
        }

        public static void OpenPackage(this Lua lua)
        {
            lua.RequireF("package", NativeLibraries.luaopen_package, true);
        }

        public static void OpenString(this Lua lua)
        {
            lua.RequireF("string", NativeLibraries.luaopen_string, true);
        }

        public static void OpenTable(this Lua lua)
        {
            lua.RequireF("table", NativeLibraries.luaopen_table, true);
        }

        public static void OpenUTF8(this Lua lua)
        {
            lua.RequireF("utf8", NativeLibraries.luaopen_utf8, true);
        }
    }
}
