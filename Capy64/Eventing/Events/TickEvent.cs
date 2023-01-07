using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Eventing.Events;

public class TickEvent : EventArgs
{
    public GameTime GameTime { get; set; }
    public ulong TotalTicks { get; set; }
}
