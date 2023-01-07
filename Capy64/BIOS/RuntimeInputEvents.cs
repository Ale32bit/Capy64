using Capy64.Eventing.Events;
using Capy64.Eventing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capy64.LuaRuntime;
using Microsoft.Xna.Framework.Input;

namespace Capy64.BIOS;

internal class RuntimeInputEvents
{
    private EventEmitter _eventEmitter;
    private Runtime _runtime;

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

        if (e.Mods.HasFlag(InputManager.Modifiers.LCtrl) || e.Mods.HasFlag(InputManager.Modifiers.RCtrl) && !e.IsHeld)
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
    }

    private void OnChar(object sender, CharEvent e)
    {
        _runtime.PushEvent("char", new object[]
        {
            e.Character,
        });
    }
}
