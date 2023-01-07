using Microsoft.Xna.Framework;
using System;

namespace Capy64.Eventing.Events;

public class MouseWheelEvent : EventArgs
{
    public Point Position { get; set; }
    public int VerticalValue { get; set; }
    public int HorizontalValue { get; set; }
}
