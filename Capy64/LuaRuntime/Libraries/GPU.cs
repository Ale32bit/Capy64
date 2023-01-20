using Capy64.API;
using KeraLua;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Capy64.LuaRuntime.Libraries;

public class GPU : IPlugin
{

    private static IGame _game;
    public GPU(IGame game)
    {
        _game = game;
    }

    private LuaRegister[] gpuLib = new LuaRegister[] {
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
            name = "getScale",
            function = L_GetScale,
        },
        new()
        {
            name = "setScale",
            function = L_SetScale,
        },
        new()
        {
            name = "getPixel",
            function = L_GetPixel,
        },
        new()
        {
            name = "plot",
            function = L_Plot,
        },
        new()
        {
            name = "plots",
            function = L_Plots,
        },
        new()
        {
            name = "drawPoint",
            function = L_DrawPoint,
        },
        new()
        {
            name = "drawCircle",
            function = L_DrawCircle
        },
        new()
        {
            name = "drawLine",
            function = L_DrawLine
        },
        new()
        {
            name = "drawRectangle",
            function = L_DrawRectangle
        },
        new()
        {
            name = "drawPolygon",
            function = L_DrawPolygon
        },
        new()
        {
            name = "drawEllipse",
            function = L_DrawEllipse
        },
        new()
        {
            name = "drawString",
            function = L_DrawString
        },
        new()
        {
            name = "measureString",
            function = L_MeasureString
        },
        new(), // NULL
    };

    public void LuaInit(Lua state)
    {
        state.RequireF("gpu", OpenLib, false);
    }

    public int OpenLib(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(gpuLib);
        return 1;
    }
    private static int L_GetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(_game.Width);
        L.PushInteger(_game.Height);

        return 2;
    }

    private static int L_SetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var w = L.CheckInteger(1);
        var h = L.CheckInteger(2);

        _game.Width = (int)w;
        _game.Height = (int)h;

        _game.UpdateSize();

        return 0;
    }

    private static int L_GetScale(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushNumber(_game.Scale);

        return 1;
    }

    private static int L_SetScale(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var s = L.CheckNumber(1);

        _game.Scale = (float)s;

        _game.UpdateSize();

        return 0;
    }

    private static int L_GetPixel(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;

        var c = _game.Drawing.GetPixel(new(x, y));

        L.PushInteger(Utils.PackRGB(c));

        return 1;
    }

    private static int L_Plot(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.Plot(new Point(x, y), new Color(r, g, b));

        return 0;
    }

    private static int L_Plots(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        var c = L.CheckInteger(2);

        L.SetTop(1);

        int size = (int)L.Length(1);

        L.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

        List<Point> pts = new();
        for (int i = 1; i <= size; i += 2)
        {
            L.GetInteger(1, i);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + i);
            var x = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            L.GetInteger(1, i + 1);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + (i + 1));
            var y = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            pts.Add(new Point(x, y));
        }

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.Plot(pts, new(r, g, b));

        return 0;
    }

    private static int L_DrawPoint(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);
        var s = (int)L.OptNumber(4, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawPoint(new(x, y), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawCircle(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var rad = (int)L.CheckNumber(3);
        var c = L.CheckInteger(4);
        var s = (int)L.OptNumber(5, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawCircle(new(x, y), rad, new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x1 = (int)L.CheckNumber(1);
        var y1 = (int)L.CheckNumber(2);
        var x2 = (int)L.CheckNumber(3);
        var y2 = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawLine(new(x1, y1), new(x2, y2), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawRectangle(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var w = (int)L.CheckNumber(3);
        var h = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawRectangle(new(x, y), new(w, h), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawPolygon(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        L.CheckType(3, LuaType.Table);
        var c = L.CheckInteger(4);
        var s = (int)L.OptNumber(5, 1);

        int size = (int)L.Length(3);

        L.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

        List<Vector2> pts = new();
        for (int i = 1; i <= size; i += 2)
        {
            L.GetInteger(3, i);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + i);
            var xp = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            L.GetInteger(3, i + 1);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + (i + 1));
            var yp = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            pts.Add(new(xp, yp));
        }

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawPolygon(new(x, y), pts.ToArray(), new(r, g, b), s);

        return 0;
    }

    private static int L_DrawEllipse(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var rx = (int)L.CheckNumber(3);
        var ry = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawEllipse(new(x, y), new(rx, ry), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);
        var t = L.CheckString(4);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawString(new Vector2(x, y), t, new Color(r, g, b));

        return 0;
    }

    private static int L_MeasureString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var t = L.CheckString(1);

        var sizes = _game.Drawing.MeasureString(t);

        L.PushNumber((int)sizes.X);
        L.PushNumber((int)sizes.Y);

        return 2;
    }
}
