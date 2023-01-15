using System;
using System.Runtime.InteropServices;

namespace Capy64.Extensions.Bindings;

public partial class SDL2
{
    private const string SDL = "SDL2.dll";

    [LibraryImport(SDL)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial void SDL_MaximizeWindow(IntPtr window);

    [LibraryImport(SDL)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial uint SDL_GetWindowFlags(IntPtr window);

    [LibraryImport(SDL, EntryPoint = "SDL_GetClipboardText")]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial IntPtr Native_SDL_GetClipboardText();

    /// <summary>
    /// </summary>
    /// <returns>0 is false; 1 is true</returns>
    [LibraryImport(SDL)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    internal static partial int SDL_HasClipboardText();

    [LibraryImport(SDL)]
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
