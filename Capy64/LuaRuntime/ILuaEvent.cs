namespace Capy64.LuaRuntime;

public interface ILuaEvent
{
    public string Name { get; set; }
    public bool BypassFilter { get; set; }
}
