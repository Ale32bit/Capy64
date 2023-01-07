using System;

namespace Capy64.Eventing.Events;

public class CharEvent : EventArgs
{
    public char Character { get; set; }
}
