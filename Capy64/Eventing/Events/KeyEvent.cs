using Microsoft.Xna.Framework.Input;
using System;
using static Capy64.Eventing.InputManager;

namespace Capy64.Eventing.Events;

public class KeyEvent : EventArgs
{
    public int KeyCode { get; set; }
    public string KeyName { get; set; }
    public bool IsHeld { get; set; }
    public Keys Key { get; set; }
    public Modifiers Mods { get; set; }
}
