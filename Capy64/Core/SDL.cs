﻿// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
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

using Capy64.Extensions.Bindings;

namespace Capy64.Core;

public class SDL
{
    public static string GetClipboardText()
    {
        return SDL2.UTF8_ToManaged(SDL2.Native_SDL_GetClipboardText(), true);
    }

    public static void SetClipboardText(string contents)
    {
        SDL2.Native_SDL_SetClipboardText(contents);
    }

    public static bool HasClipboardText()
    {
        return SDL2.SDL_HasClipboardText() == 1;
    }
}
