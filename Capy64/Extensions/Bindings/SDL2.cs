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

namespace Capy64.Extensions.Bindings;

public partial class SDL2
{
#if _WINDOWS
    private const string LibraryName = "SDL2.dll";
#elif _LINUX
    private const string LibraryName = "libSDL2-2.0.so.0";
#elif _OSX
    private const string LibraryName = "libSDL2.dylib";
#else
    private const string LibraryName = "SDL2";
#endif

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void SDL_MaximizeWindow(IntPtr window);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial uint SDL_GetWindowFlags(IntPtr window);

    [LibraryImport(LibraryName, EntryPoint = "SDL_GetClipboardText")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr Native_SDL_GetClipboardText();

    /// <summary>
    /// </summary>
    /// <returns>0 is false; 1 is true</returns>
    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial int SDL_HasClipboardText();

    [LibraryImport(LibraryName, EntryPoint = "SDL_SetClipboardText", StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial int Native_SDL_SetClipboardText(string contents);

    [LibraryImport(LibraryName)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void SDL_free(IntPtr memblock);

    /// <summary>
    /// https://github.com/flibitijibibo/SDL2-CS/blob/master/src/SDL2.cs#L128
    /// </summary>
    /// <param name="s"></param>
    /// <param name="freePtr"></param>
    /// <returns></returns>
    public static unsafe string UTF8_ToManaged(IntPtr s, bool freePtr = false)
    {
        if (s == IntPtr.Zero)
        {
            return null;
        }

        /* We get to do strlen ourselves! */
        byte* ptr = (byte*)s;
        while (*ptr != 0)
        {
            ptr++;
        }

        /* TODO: This #ifdef is only here because the equivalent
         * .NET 2.0 constructor appears to be less efficient?
         * Here's the pretty version, maybe steal this instead:
         *
        string result = new string(
            (sbyte*) s, // Also, why sbyte???
            0,
            (int) (ptr - (byte*) s),
            System.Text.Encoding.UTF8
        );
         * See the CoreCLR source for more info.
         * -flibit
         */
#if NETSTANDARD2_0
			/* Modern C# lets you just send the byte*, nice! */
			string result = System.Text.Encoding.UTF8.GetString(
				(byte*) s,
				(int) (ptr - (byte*) s)
			);
#else
        /* Old C# requires an extra memcpy, bleh! */
        int len = (int)(ptr - (byte*)s);
        if (len == 0)
        {
            return string.Empty;
        }
        char* chars = stackalloc char[len];
        int strLen = System.Text.Encoding.UTF8.GetChars((byte*)s, len, chars, len);
        string result = new string(chars, 0, strLen);
#endif

        /* Some SDL functions will malloc, we have to free! */
        if (freePtr)
        {
            SDL_free(s);
        }
        return result;
    }
}
