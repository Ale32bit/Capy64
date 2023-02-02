using Capy64.API;
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Integrations;

public class DiscordIntegration : IPlugin
{
    public DiscordRpcClient Client { get; private set; }
    private readonly IConfiguration _configuration;

    public DiscordIntegration(IConfiguration configuration)
    {
        _configuration = configuration;

        var discordConfig = _configuration.GetSection("Integrations:Discord");
        Client = new(discordConfig["ApplicationId"]);

#if DEBUG
        Client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Info };
        Client.OnReady += OnReady;
        Client.OnPresenceUpdate += OnPresenceUpdate;
#endif

        Capy64.Instance.Discord = this;

        if (discordConfig.GetValue("Enable", false))
        {
            Client.Initialize();
        }
    }

    public void SetPresence(string details, string? state = null)
    {
        Client.SetPresence(new RichPresence()
        {
            Details = details,
            State = state,
            Timestamps = Timestamps.Now,
            Assets = new Assets()
            {
                LargeImageKey = "image_large",
                LargeImageText = "Capy64 " + Capy64.Version,
                SmallImageKey = "image_small"
            }
        });
    }

    private void OnReady(object sender, ReadyMessage e)
    {
        Console.WriteLine("Received Ready from user {0}", e.User.Username);
    }

    private void OnPresenceUpdate(object sender, PresenceMessage e)
    {
        Console.WriteLine("Received Update! {0}", e.Presence);
    }
}
