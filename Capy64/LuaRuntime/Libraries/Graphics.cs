using Capy64.API;
using KeraLua;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Capy64.LuaRuntime.Libraries;

public class Graphics : IPlugin
{
    private static IGame _game;
    public Graphics(IGame game)
    {
        _game = game;
    }

    private LuaRegister[] GfxLib = new LuaRegister[] {
        new()
        {
            name = "drawPoint",
            function = L_DrawPoint,
        },
        new()
        {
            name = "drawPoints",
            function = L_DrawPoints,
        },
        new(), // NULL
    };

    public void LuaInit(Lua state)
    {
        state.RequireF("graphics", Open, false);
    }

    public int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(GfxLib);
        return 1;
    }

    private static int L_DrawPoint(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = L.CheckInteger(1) - 1;
        var y = L.CheckInteger(2) - 1;
        var c = L.CheckInteger(3);
        var s = L.OptInteger(4, 1);

        Utils.UnpackRGB((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawPoint(new(x, y), new Color(r, g, b), (int)s);

        return 0;
    }

    private static int L_DrawPoints(IntPtr state)
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
}
