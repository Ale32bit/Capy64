using Microsoft.Xna.Framework;
using System;

namespace Capy64.Eventing.Events;

public class TickEvent : EventArgs
{
    public GameTime GameTime { get; set; }
    public ulong TotalTicks { get; set; }
}
