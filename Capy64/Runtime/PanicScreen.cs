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

using Capy64.Core;
using Capy64.Runtime.Libraries;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class PanicScreen
{
    public static Color ForegroundColor = Color.White;
    public static Color BackgroundColor = new Color(0, 51, 187);

    public static void Render(string error, string details = null)
    {
        Term.ForegroundColor = ForegroundColor;
        Term.BackgroundColor = BackgroundColor;
        Term.SetCursorBlink(false);
        Term.SetSize(51, 19);
        Term.Clear();

        var title = " Capy64 ";
        var halfX = (Term.Width / 2) + 1;
        Term.SetCursorPosition(halfX - (title.Length / 2), 2);
        Term.ForegroundColor = BackgroundColor;
        Term.BackgroundColor = ForegroundColor;
        Term.Write(title);

        Term.ForegroundColor = ForegroundColor;
        Term.BackgroundColor = BackgroundColor;
        Term.SetCursorPosition(1, 4);
        Print(error + '\n');

        if (details is not null)
        {
            Print(details);
        }
        Term.SetCursorPosition(1, 19);
        Print("Hold CTRL + ALT + INSERT to reboot.");
    }

    private static void Print(string txt)
    {
        foreach (var ch in txt)
        {
            Term.Write(ch.ToString());
            if (Term.CursorPosition.X >= Term.Width || ch == '\n')
            {
                Term.SetCursorPosition(1, (int)Term.CursorPosition.Y + 1);
            }
        }
        Term.SetCursorPosition(1, (int)Term.CursorPosition.Y + 1);
    }
}
