using System;
using System.Runtime.InteropServices;

namespace Capy64.Extensions.Bindings;

public partial class SDL2
{
    private const string SDL = "SDL2.dll";

    [LibraryImport(SDL)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial void SDL_MaximizeWindow(IntPtr window);

    [LibraryImport(SDL)]
    [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static partial uint SDL_GetWindowFlags(IntPtr window);
}
