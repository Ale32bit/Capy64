using Capy64.API;
using Capy64.Eventing.Events;
using KeraLua;
using Microsoft.Xna.Framework;
using System;

namespace Capy64.LuaRuntime.Libraries;

internal class Term : IPlugin
{
    private struct Char
    {
        public char Character;
        public Color Foreground;
        public Color Background;
    }

    public const int CharWidth = 7;
    public const int CharHeight = 13;

    public const int CursorDelay = 30;

    public static int Width { get; private set; } = 57;
    public static int Height { get; private set; } = 23;
    public static int RealWidth => CharWidth * Width;
    public static int RealHeight => CharHeight * Height;
    public static Vector2 CharOffset => new(1, 0);
    public static Vector2 CursorPosition => _cursorPosition + Vector2.One;
    private static Vector2 _cursorPosition { get; set; }
    public static Color ForegroundColor { get; set; }
    public static Color BackgroundColor { get; set; }
    private static Char?[] CharGrid;

    private static IGame _game;
    private static bool cursorState = false;
    private static bool enableCursor = true;
    public Term(IGame game)
    {
        _game = game;

        _cursorPosition = Vector2.Zero;
        ForegroundColor = Color.White;
        BackgroundColor = Color.Black;

        UpdateSize();

        _game.EventEmitter.OnTick += OnTick;
    }

    private LuaRegister[] TermLib = new LuaRegister[]
    {
        new()
        {
            name = "write",
            function = L_Write,
        },
        new()
        {
            name = "getPos",
            function = L_GetPos,
        },
        new()
        {
            name = "setPos",
            function = L_SetPos,
        },
        new()
        {
            name = "getSize",
            function = L_GetSize,
        },
        new()
        {
            name = "setSize",
            function = L_SetSize,
        },
        new()
        {
            name = "getForeground",
            function = L_GetForegroundColor,
        },
        new()
        {
            name = "setForeground",
            function = L_SetForegroundColor,
        },
        new()
        {
            name = "getBackground",
            function = L_GetBackgroundColor,
        },
        new()
        {
            name = "setBackground",
            function = L_SetBackgroundColor,
        },
        new()
        {
            name = "toRealPos",
            function = L_ToReal,
        },
        new()
        {
            name = "fromRealPos",
            function = L_FromReal,
        },
        new()
        {
            name = "clear",
            function = L_Clear,
        },
        new()
        {
            name = "clearLine",
            function = L_ClearLine,
        },
        new()
        {
            name = "scroll",
            function = L_Scroll,
        },
        new()
        {
            name = "getBlink",
            function = L_GetBlink,
        },
        new()
        {
            name = "setBlink",
            function = L_SetBlink,
        },
        new(),
    };

    public void LuaInit(Lua state)
    {
        state.RequireF("term", Open, false);
    }

    public int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(TermLib);
        return 1;
    }

    public static void UpdateSize()
    {
        _game.Width = RealWidth;
        _game.Height = RealHeight;
        _game.UpdateSize();
        CharGrid = new Char?[Width * Height];
    }

    public static Vector2 ToRealPos(Vector2 termPos)
    {
        return new(termPos.X * CharWidth, termPos.Y * CharHeight);
    }

    public static void PlotChar(Vector2 pos, char ch, Color? fgc = null, Color? bgc = null, bool save = true)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Width || pos.Y >= Height)
            return;

        var fg = fgc ?? ForegroundColor;
        var bg = bgc ?? BackgroundColor;

        var realpos = ToRealPos(pos);
        var charpos = realpos + CharOffset;
        _game.Drawing.DrawRectangle(realpos, new(CharWidth, CharHeight), bg, Math.Min(CharWidth, CharHeight));
        _game.Drawing.DrawString(charpos, ch.ToString(), fg);

        if (!save)
            return;

        CharGrid[(int)pos.X + ((int)pos.Y * Width)] = new Char
        {
            Character = ch,
            Foreground = ForegroundColor,
            Background = BackgroundColor,
        };
    }

    public static void RedrawPos(Vector2 pos)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Width || pos.Y >= Height)
            return;

        var ch = CharGrid[(int)pos.X + ((int)pos.Y * Width)] ??
            new Char
            {
                Character = ' ',
                Foreground = ForegroundColor,
                Background = BackgroundColor,
            };

        PlotChar(pos, ch.Character, ch.Foreground, ch.Background, false);
    }

    public static void RedrawAll()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                RedrawPos(new(x, y));
            }
        }
    }

    public static void DumpScreen(bool clear = false)
    {
        if (clear)
            Console.Clear();
        Console.WriteLine("\n   /{0}", new string('-', Width));
        for (int i = 0; i < CharGrid.Length; i++)
        {
            if (i % Width == 0)
            {
                if (i > 0)
                    Console.Write("|\n");
                Console.Write("{0,3}:", i / Width);
            }
            Console.Write(CharGrid[i]?.Character ?? ' ');

        }
        Console.WriteLine("|\n   \\{0}", new string('-', Width));
    }

    private void OnTick(object sender, TickEvent e)
    {
        if (((int)e.TotalTicks % CursorDelay) == 0)
        {
            cursorState = !cursorState;
            UpdateCursor();
        }
    }

    private static void UpdateCursor()
    {
        if (!enableCursor)
            return;

        if (_cursorPosition.X < 0 || _cursorPosition.Y < 0 || _cursorPosition.X >= Width || _cursorPosition.Y >= Height)
            return;

        var ch = CharGrid[(int)_cursorPosition.X + ((int)_cursorPosition.Y * Width)] ??
            new Char
            {
                Character = ' ',
                Foreground = ForegroundColor,
                Background = BackgroundColor,
            };

        Color fg, bg;

        if (cursorState)
        {
            fg = ch.Background;
            bg = ch.Foreground;
        }
        else
        {
            fg = ch.Foreground;
            bg = ch.Background;
        }

        PlotChar(_cursorPosition, ch.Character, fg, bg, false);
    }

    private static void ClearGrid()
    {
        CharGrid = new Char?[CharGrid.Length];
        _game.Drawing.Clear(BackgroundColor);
    }

    public static void Write(string text)
    {
        foreach (var ch in text)
        {
            PlotChar(_cursorPosition, ch);
            _cursorPosition = new(_cursorPosition.X + 1, _cursorPosition.Y);
        }
    }

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var str = L.ToString(1);

        Write(str);

        return 0;
    }

    public static void SetCursorPosition(int x, int y)
    {
        RedrawPos(_cursorPosition);
        cursorState = true;
        _cursorPosition = new(x - 1, y - 1);
        UpdateCursor();
    }

    private static int L_GetPos(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger((int)_cursorPosition.X + 1);
        L.PushInteger((int)_cursorPosition.Y + 1);

        return 2;
    }

    private static int L_SetPos(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = L.CheckNumber(1);
        var y = L.CheckNumber(2);

        SetCursorPosition((int)x, (int)y);

        return 0;
    }

    public static void SetSize(int width, int height)
    {
        Width = width;
        Height = height;

        UpdateSize();
    }

    private static int L_GetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(Width);
        L.PushInteger(Height);

        return 2;
    }

    private static int L_SetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var w = (int)L.CheckNumber(1);
        var h = (int)L.CheckNumber(2);

        if (w <= 0)
        {
            L.ArgumentError(1, "number must be greater than 0");
        }
        if (h <= 0)
        {
            L.ArgumentError(2, "number must be greater than 0");
        }

        SetSize(w, h);

        return 0;
    }

    private static int L_GetForegroundColor(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(ForegroundColor.R);
        L.PushInteger(ForegroundColor.G);
        L.PushInteger(ForegroundColor.B);

        return 3;
    }

    private static int L_SetForegroundColor(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var argsn = L.GetTop();

        byte r, g, b;
        // R, G, and B values
        if (argsn == 3)
        {
            r = (byte)L.CheckNumber(1);
            g = (byte)L.CheckNumber(2);
            b = (byte)L.CheckNumber(3);
        }
        // packed RGB value
        else if (argsn == 1)
        {
            var c = (uint)L.CheckInteger(1);
            Utils.UnpackRGB(c, out r, out g, out b);
        }
        else
        {
            L.ArgumentError(argsn, "expected 1 or 3 number values");
            return 0;
        }

        ForegroundColor = new(r, g, b);

        return 0;
    }

    private static int L_GetBackgroundColor(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(BackgroundColor.R);
        L.PushInteger(BackgroundColor.G);
        L.PushInteger(BackgroundColor.B);

        return 3;
    }

    private static int L_SetBackgroundColor(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var argsn = L.GetTop();

        byte r, g, b;
        // R, G, and B values
        if (argsn == 3)
        {
            r = (byte)L.CheckNumber(1);
            g = (byte)L.CheckNumber(2);
            b = (byte)L.CheckNumber(3);
        }
        // packed RGB value
        else if (argsn == 1)
        {
            var c = (uint)L.CheckInteger(1);
            Utils.UnpackRGB(c, out r, out g, out b);
        }
        else
        {
            L.ArgumentError(argsn, "expected 1 or 3 number values");
            return 0;
        }

        BackgroundColor = new(r, g, b);

        return 0;
    }

    private static int L_ToReal(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1);
        var y = (int)L.CheckNumber(2);

        L.PushInteger((x * CharWidth) - CharWidth + 1);
        L.PushInteger((y * CharHeight) - CharHeight + 1);

        return 2;
    }

    private static int L_FromReal(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;

        L.PushInteger((x / CharWidth) + 1);
        L.PushInteger((y / CharHeight) + 1);

        return 2;
    }

    public static void Clear()
    {
        ClearGrid();

        RedrawAll();
    }

    private static int L_Clear(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        Clear();

        return 0;
    }

    public static void ClearLine()
    {
        for (int x = 0; x < Width; x++)
            PlotChar(new(x, _cursorPosition.Y), ' ');
    }

    private static int L_ClearLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        ClearLine();

        return 0;
    }

    public static void Scroll(int lines)
    {
        if (lines == 0)
            return;

        if (lines <= -Height || lines >= Height)
        {
            ClearGrid();
            return;
        }

        var lineLength = Math.Abs(lines) * Width;
        var newGrid = new Char?[CharGrid.Length];
        if (lines < 0)
        {
            Array.Copy(CharGrid, lineLength, newGrid, 0, CharGrid.Length - lineLength);
        }
        else
        {
            Array.Copy(CharGrid, 0, newGrid, lineLength, CharGrid.Length - lineLength);
        }

        CharGrid = newGrid;

        RedrawAll();
    }

    private static int L_Scroll(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var lines = L.CheckInteger(1) * -1;

        Scroll((int)lines);

        return 0;
    }

    public static void SetCursorBlink(bool enable)
    {
        enableCursor = enable;

        if (!enableCursor)
        {
            RedrawPos(_cursorPosition);
        }
    }

    private static int L_GetBlink(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushBoolean(enableCursor);

        return 1;
    }

    private static int L_SetBlink(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Boolean);
        var enableCursor = L.ToBoolean(1);

        SetCursorBlink(enableCursor);

        return 0;
    }
}
