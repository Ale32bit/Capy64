using System;
using System.Runtime.InteropServices;

namespace Capy64.LuaRuntime.Extensions
{
    internal static partial class NativeLibraries
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

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_base(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_coroutine(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_debug(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_io(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_math(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_os(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_package(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_string(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_table(IntPtr luaState);

        [LibraryImport(LuaLibraryName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
        internal static partial int luaopen_utf8(IntPtr luaState);
    }
}
