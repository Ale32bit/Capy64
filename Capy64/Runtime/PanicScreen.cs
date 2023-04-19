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

using Capy64.Runtime.Libraries;
using Microsoft.Xna.Framework;

namespace Capy64.Runtime;

public class PanicScreen
{
    public static Color ForegroundColor = Color.White;
    public static Color BackgroundColor = new(0, 51, 187);

    public static void Render(string error, string details = null)
    {
        TermLib.ForegroundColor = ForegroundColor;
        TermLib.BackgroundColor = BackgroundColor;
        TermLib.SetCursorBlink(false);
        TermLib.SetSize(57, 23);
        TermLib.Clear();

        var title = " Capy64 ";
        var halfX = (TermLib.Width / 2) + 1;
        TermLib.SetCursorPosition(halfX - (title.Length / 2), 2);
        TermLib.ForegroundColor = BackgroundColor;
        TermLib.BackgroundColor = ForegroundColor;
        TermLib.Write(title);

        TermLib.ForegroundColor = ForegroundColor;
        TermLib.BackgroundColor = BackgroundColor;
        TermLib.SetCursorPosition(1, 4);
        Print(error + '\n');

        if (details is not null)
        {
            Print(details);
        }
        TermLib.SetCursorPosition(1, 23);
        Print("Hold CTRL + ALT + INSERT to reboot.");
    }

    private static void Print(string txt)
    {
        foreach (var ch in txt)
        {
            TermLib.Write(ch.ToString());
            if (TermLib.CursorPosition.X >= TermLib.Width || ch == '\n')
            {
                TermLib.SetCursorPosition(1, (int)TermLib.CursorPosition.Y + 1);
            }
        }
        TermLib.SetCursorPosition(1, (int)TermLib.CursorPosition.Y + 1);
    }
}
