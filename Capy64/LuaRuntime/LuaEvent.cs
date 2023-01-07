namespace Capy64.LuaRuntime;

public struct LuaEvent
{
    public string Name { get; set; }
    public object[] Parameters { get; set; }
    public bool BypassFilter { get; set; }
}
