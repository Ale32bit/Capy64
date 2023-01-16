using Capy64.Core;
using Capy64.Eventing;
using Capy64.Eventing.Events;
using Capy64.LuaRuntime;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using static Capy64.Eventing.InputManager;

namespace Capy64.BIOS;

internal class RuntimeInputEvents
{
    private EventEmitter _eventEmitter;
    private Runtime _runtime;
    private const int rebootDelay = 30;
    private int heldReboot = 0;

    public RuntimeInputEvents(EventEmitter eventEmitter, Runtime runtime)
    {
        _eventEmitter = eventEmitter;
        _runtime = runtime;
    }

    public void Register()
    {
        _eventEmitter.OnMouseUp += OnMouseUp;
        _eventEmitter.OnMouseDown += OnMouseDown;
        _eventEmitter.OnMouseMove += OnMouseMove;
        _eventEmitter.OnMouseWheel += OnMouseWheel;

        _eventEmitter.OnKeyUp += OnKeyUp;
        _eventEmitter.OnKeyDown += OnKeyDown;
        _eventEmitter.OnChar += OnChar;

        _eventEmitter.OnTick += OnTick;
    }

    public void Unregister()
    {
        _eventEmitter.OnMouseUp -= OnMouseUp;
        _eventEmitter.OnMouseDown -= OnMouseDown;
        _eventEmitter.OnMouseMove -= OnMouseMove;
        _eventEmitter.OnMouseWheel -= OnMouseWheel;

        _eventEmitter.OnKeyUp -= OnKeyUp;
        _eventEmitter.OnKeyDown -= OnKeyDown;
        _eventEmitter.OnChar -= OnChar;

        _eventEmitter.OnTick -= OnTick;
    }

    private static Keys[] rebootKeys = new[]
    {
        Keys.Insert,
        Keys.LeftAlt, Keys.RightAlt,
        Keys.LeftControl, Keys.RightControl,
    };
    private void OnTick(object sender, TickEvent e)
    {
        var keyState = Keyboard.GetState();
        var pressedKeys = keyState.GetPressedKeys();

        if ((pressedKeys.Contains(Keys.LeftControl) || pressedKeys.Contains(Keys.RightControl))
            && (pressedKeys.Contains(Keys.LeftAlt) || pressedKeys.Contains(Keys.RightAlt))
            && pressedKeys.Contains(Keys.Insert))
        {
            heldReboot++;
        }
        else
        {
            heldReboot = 0;
        }

        if (heldReboot >= rebootDelay)
        {
            heldReboot = 0;
            Bios.Reboot();
        }
    }

    private void OnMouseUp(object sender, MouseButtonEvent e)
    {
        _runtime.PushEvent("mouse_up", new object[]
        {
            (int)e.Button,
            e.Position.X,
            e.Position.Y,
        });
    }
    private void OnMouseDown(object sender, MouseButtonEvent e)
    {
        _runtime.PushEvent("mouse_down", new object[]
        {
            (int)e.Button,
            e.Position.X,
            e.Position.Y,
        });
    }

    private void OnMouseMove(object sender, MouseMoveEvent e)
    {
        _runtime.PushEvent("mouse_move", new object[]
        {
            e.PressedButtons,
            e.Position.X,
            e.Position.Y,
        });
    }
    private void OnMouseWheel(object sender, MouseWheelEvent e)
    {
        _runtime.PushEvent("mouse_scroll", new object[]
        {
            e.Position.X,
            e.Position.Y,
            e.VerticalValue,
            e.HorizontalValue,
        });
    }

    private void OnKeyUp(object sender, KeyEvent e)
    {
        _runtime.PushEvent("key_up", new object[]
        {
            e.KeyCode,
            e.KeyName,
            (int)e.Mods
        });
    }
    private void OnKeyDown(object sender, KeyEvent e)
    {
        _runtime.PushEvent("key_down", new object[]
        {
            e.KeyCode,
            e.KeyName,
            (int)e.Mods,
            e.IsHeld,
        });

        if ((e.Mods & Modifiers.Ctrl) != Modifiers.None && !e.IsHeld)
        {
            if ((e.Mods & Modifiers.Alt) != Modifiers.None)
            {
                if (e.Key == Keys.C)
                {
                    _runtime.PushEvent(new LuaEvent()
                    {
                        Name = "interrupt",
                        Parameters = { },
                        BypassFilter = true,
                    });
                }
            }
            else if (e.Key == Keys.V)
            {
                if (SDL.HasClipboardText())
                {
                    var text = SDL.GetClipboardText();
                    _runtime.PushEvent(new LuaEvent()
                    {
                        Name = "paste",
                        Parameters = new[] {
                            text,
                        },
                    });
                }
            }
        }
    }

    private void OnChar(object sender, CharEvent e)
    {
        _runtime.PushEvent("char", new object[]
        {
            e.Character,
        });
    }
}
