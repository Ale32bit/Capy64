namespace Capy64.LuaRuntime;

public class LuaEvent : ILuaEvent
{
    public string Name { get; set; }
    public object[] Parameters { get; set; }
    public bool BypassFilter { get; set; } = false;
}
