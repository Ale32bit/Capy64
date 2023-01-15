using Capy64.Extensions.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Core;

public class SDL
{
    public static string GetClipboardText()
    {
        return SDL2.UTF8_ToManaged(SDL2.Native_SDL_GetClipboardText(), true);
    }

    public static bool HasClipboardText()
    {
        return SDL2.SDL_HasClipboardText() == 1;
    }
}
