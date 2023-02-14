using Microsoft.Xna.Framework.Input;
using System;

namespace Capy64.Eventing.Events;

public class GamePadButtonEvent : EventArgs
{
    public Buttons Button { get; set; }
    public ButtonState State { get; set; }
}
