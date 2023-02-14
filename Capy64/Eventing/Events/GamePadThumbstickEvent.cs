using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Capy64.Eventing.Events;

public class GamePadThumbstickEvent : EventArgs
{
    public int Stick { get; set; }
    public Vector2 Value { get; set; }
}
