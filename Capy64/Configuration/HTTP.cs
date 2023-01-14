namespace Capy64.Configuration;

class HTTP
{
    public bool Enable { get; set; } = true;
    public string[] Blacklist { get; set; }
    public WebSockets WebSockets { get; set; }
}
