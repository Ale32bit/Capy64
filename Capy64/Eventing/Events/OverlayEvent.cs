using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Capy64.Eventing.Events;

public class OverlayEvent : EventArgs
{
    public GameTime GameTime { get; set; }
    public ulong TotalTicks { get; set; }
}
