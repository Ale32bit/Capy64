// This file is part of Capy64 - https://github.com/Capy64/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Capy64.Core;
using Capy64.Eventing.Events;
using Capy64.Eventing;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using static Capy64.Eventing.InputManager;
using Capy64.Runtime.Extensions;

namespace Capy64.Runtime;

internal class EventEmitter
{
    private Eventing.EventEmitter _eventEmitter;
    private LuaState _runtime;
    private const int rebootDelay = 30;
    private int heldReboot = 0;

    public EventEmitter(Eventing.EventEmitter eventEmitter, LuaState runtime)
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
        _eventEmitter.OnScreenSizeChange += OnScreenSizeChange;

        _eventEmitter.OnGamePadButton += OnGamePadButton;
        _eventEmitter.OnGamePadTrigger += OnGamePadTrigger;
        _eventEmitter.OnGamePadThumbstick += OnGamePadThumbstick;
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
        _eventEmitter.OnScreenSizeChange -= OnScreenSizeChange;

        _eventEmitter.OnGamePadButton -= OnGamePadButton;
        _eventEmitter.OnGamePadTrigger -= OnGamePadTrigger;
        _eventEmitter.OnGamePadThumbstick -= OnGamePadThumbstick;
    }

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
            RuntimeManager.ResetPanic();
            RuntimeManager.Reset();
        }
    }

    private void OnMouseUp(object sender, MouseButtonEvent e)
    {
        _runtime.QueueEvent("mouse_up", LK =>
        {
            LK.PushInteger((int)e.Button);
            LK.PushInteger(e.Position.X);
            LK.PushInteger(e.Position.Y);

            return 3;
        });
    }
    private void OnMouseDown(object sender, MouseButtonEvent e)
    {
        _runtime.QueueEvent("mouse_down", LK =>
        {
            LK.PushInteger((int)e.Button);
            LK.PushInteger(e.Position.X);
            LK.PushInteger(e.Position.Y);

            return 3;
        });
    }

    private void OnMouseMove(object sender, MouseMoveEvent e)
    {
        _runtime.QueueEvent("mouse_move", LK =>
        {
            LK.NewTable();
            for (int i = 1; i <= e.PressedButtons.Length; i++)
            {
                LK.PushInteger(e.PressedButtons[i - 1]);
                LK.SetInteger(-2, i);

            }
            LK.PushInteger(e.Position.X);
            LK.PushInteger(e.Position.Y);

            return 3;
        });
    }
    private void OnMouseWheel(object sender, MouseWheelEvent e)
    {
        _runtime.QueueEvent("mouse_scroll", LK =>
        {
            LK.PushInteger(e.Position.X);
            LK.PushInteger(e.Position.Y);
            LK.PushInteger(e.VerticalValue);
            LK.PushInteger(e.HorizontalValue);

            return 4;
        });
    }

    private void OnKeyUp(object sender, KeyEvent e)
    {
        _runtime.QueueEvent("key_up", LK =>
        {
            LK.PushInteger(e.KeyCode);
            LK.PushString(e.KeyName);
            LK.PushInteger((int)e.Mods);

            return 3;
        });
    }
    private void OnKeyDown(object sender, KeyEvent e)
    {
        _runtime.QueueEvent("key_down", LK =>
        {
            LK.PushInteger(e.KeyCode);
            LK.PushString(e.KeyName);
            LK.PushInteger((int)e.Mods);
            LK.PushBoolean(e.IsHeld);

            return 4;
        });

        if ((e.Mods & Modifiers.Ctrl) != Modifiers.None && !e.IsHeld)
        {
            if ((e.Mods & Modifiers.Alt) != Modifiers.None)
            {
                if (e.Key == Keys.C)
                    _runtime.QueueEvent("interrupt", LK => 0);
            }
            else if (e.Key == Keys.V)
            {
                if (SDL.HasClipboardText())
                {
                    var text = SDL.GetClipboardText();
                    _runtime.QueueEvent("paste", LK => {
                        LK.PushString(text);

                        return 1;
                    });
                }
            }
        }
    }

    private void OnChar(object sender, CharEvent e)
    {
        _runtime.QueueEvent("char", LK => {
            LK.PushString(e.Character.ToString());

            return 1;
        });
    }

    private void OnScreenSizeChange(object sender, EventArgs e)
    {
        _runtime.QueueEvent("screen_resize", LK =>
        {
            return 0;
        });
    }

    private void OnGamePadButton(object sender, GamePadButtonEvent e)
    {
        _runtime.QueueEvent("gamepad_button", LK =>
        {
            LK.PushInteger((int)e.Button);
            LK.PushBoolean(e.State == ButtonState.Pressed);
            return 2;
        });
    }

    private void OnGamePadTrigger(object sender, GamePadTriggerEvent e)
    {
        _runtime.QueueEvent("gamepad_trigger", LK =>
        {
            LK.PushInteger(e.Trigger);
            LK.PushNumber(e.Value);
            return 2;
        });
    }

    private void OnGamePadThumbstick(object sender, GamePadThumbstickEvent e)
    {
        _runtime.QueueEvent("gamepad_stick", LK =>
        {
            LK.PushInteger(e.Stick);
            LK.PushNumber(e.Value.X);
            LK.PushNumber(e.Value.Y);
            return 2;
        });
    }
}
