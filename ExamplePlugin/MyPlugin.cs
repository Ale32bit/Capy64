using Capy64;
using Capy64.API;
using KeraLua;

namespace ExamplePlugin;

public class MyPlugin : IComponent
{
    private static IGame _game;
    public MyPlugin(IGame game)
    {
        _game = game;
    }


    private static LuaRegister[] MyLib = new LuaRegister[]
    {
        new()
        {
            name = "hello",
            function = L_HelloWorld,
        },

        new(),
    };
    public void LuaInit(Lua L)
    {
        L.RequireF("mylib", OpenLib, false);
    }

    private static int OpenLib(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.NewLib(MyLib);

        return 1;
    }

    private static int L_HelloWorld(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushString("Hello, World");

        return 1;
    }
}