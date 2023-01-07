using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using static Capy64.Eventing.InputManager;

namespace Capy64.Eventing.Events;

public class MouseButtonEvent : EventArgs
{
    public MouseButton Button { get; set; }
    public ButtonState State { get; set; }
    public Point Position { get; set; }
}
