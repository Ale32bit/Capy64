namespace Capy64.Configuration;

class WebSockets
{
    public bool Enable { get; set; } = true;
    public int MaxActiveConnections { get; set; } = 5;
}
