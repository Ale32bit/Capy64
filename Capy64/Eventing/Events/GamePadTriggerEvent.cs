using Microsoft.Xna.Framework.Input;
using System;

namespace Capy64.Eventing.Events;

public class GamePadTriggerEvent : EventArgs
{
    public int Trigger { get; set; }
    public float Value { get; set; }
}
