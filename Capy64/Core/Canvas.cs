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

using Capy64.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using static SDL2.SDL;
using static SDL2.SDL_ttf;

namespace Capy64.Core;
public class Canvas : IDisposable
{
    public nint Font { get; private set; }
    private readonly Game _game;
    private nint _surface => _game.VideoSurface;
    private nint _renderer => _game.SurfaceRenderer;
    public Canvas(Game game)
    {
        _game = game;
        //File.ReadAllBytes(Path.Combine(Game.AssetsPath, "font.ttf"));
        Font = TTF_OpenFont(Path.Combine(Game.AssetsPath, "font.ttf"), 13);
    }

    public void SetPixel(int x, int y, Color color) {
        SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A);
        SDL_RenderDrawPoint(_renderer, x, y);
    }
    public void SetPixel(Point point, Color color)
    {
        SetPixel(point.X, point.Y, color);
    }
    public Color GetPixel(int x, int y)
    {
        unsafe {
            var pitch = ((SDL_Surface*)_surface)->pitch;
            var c = ((uint*)((SDL_Surface*)_surface)->pixels)[x + y * pitch / 4];
            return new Color(c);
        }
    }
    public Color GetPixel(Point point)
    {
        return GetPixel(point.X, point.Y);
    }
    public void SetPixels(IEnumerable<Point> points, Color color)
    {

    }

    public void DrawRectangle(Vector2 pos, Size2 size, Color color, int thickness = 1, float rotation = 0f)
    {
        var rect = new SDL_FRect
        {
            x = pos.X,
            y = pos.Y,
            w = size.Width,
            h = size.Height,
        };

        SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A);
        SDL_RenderDrawRectF(_renderer, ref rect);
        //DrawRectangle(new(pos, size), color, thickness, rotation, 0f);
    }


    public void DrawFilledRectangle(Vector2 pos, Size2 size, Color color)
    {
        var rect = new SDL_FRect
        {
            x = pos.X,
            y = pos.Y,
            w = size.Width,
            h = size.Height,
        };

        SDL_SetRenderDrawColor(_renderer, color.R, color.G, color.B, color.A);
        SDL_RenderFillRectF(_renderer, ref rect);
    }

    public void DrawString(Vector2 charpos, string v, Color fg)
    {
        var surf = TTF_RenderText_Solid(Font, v, fg.ToSDL());
        var message = SDL_CreateTextureFromSurface(_renderer, surf);
        var rect = new SDL_Rect
        {
            x = (int)charpos.X,
            y = (int)charpos.Y,
            w = 9,
            h = 13,
        };

        SDL_SetRenderDrawColor(_surface, 255, 255, 255, 255);
        SDL_RenderCopy(_renderer, message, 0, ref rect);

        SDL_DestroyTexture(message);
        SDL_FreeSurface(surf);
    }

    public void Clear(Color? color = default)
    {
        var clearColor = color ?? Color.Black;
        SDL_SetRenderDrawColor(_renderer, clearColor.R, clearColor.G, clearColor.B, 255);
        SDL_RenderClear(_renderer);
    }

    public void Dispose()
    {
        TTF_CloseFont(Font);
    }
}
