using KeraLua;

namespace Capy64.LuaRuntime.Objects.Handlers;

public interface IHandle
{
    public void Push(Lua L, bool newTable = true);
}
