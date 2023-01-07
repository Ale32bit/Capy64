using Microsoft.Xna.Framework;
using System;

namespace Capy64.Eventing.Events;

public class MouseMoveEvent : EventArgs
{
    public Point Position { get; set; }
    public int[] PressedButtons { get; set; }
}
