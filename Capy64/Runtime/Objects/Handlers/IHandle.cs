using KeraLua;

namespace Capy64.Runtime.Objects.Handlers;

public interface IHandle
{
    public void Push(Lua L, bool newTable = true);
}
