// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.InteropServices;

namespace Capy64.Runtime.Extensions
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
