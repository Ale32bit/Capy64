﻿// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capy64.Eventing;

public class InputManager
{
    public enum MouseButton
    {
        Left = 1,
        Right = 2,
        Middle = 3,
        Button4 = 4,
        Button5 = 5,
    }

    [Flags]
    public enum Modifiers
    {
        None = 0,

        LShift = 1,
        RShift = 2,

        LAlt = 4,
        RAlt = 8,

        LCtrl = 16,
        RCtrl = 32,

        Shift = LShift | RShift,
        Alt = LAlt | RAlt,
        Ctrl = LCtrl | RCtrl,
    }

    private static readonly Keys[] IgnoredTextInputKeys =
    {
        Keys.Enter,
        Keys.Back,
        Keys.Tab,
        Keys.Delete,
        Keys.Escape,
        Keys.VolumeDown,
        Keys.VolumeUp,
        Keys.VolumeMute,
    };

    private readonly Dictionary<MouseButton, ButtonState> mouseButtonStates = new()
    {
        [MouseButton.Left] = ButtonState.Released,
        [MouseButton.Right] = ButtonState.Released,
        [MouseButton.Middle] = ButtonState.Released,
        [MouseButton.Button4] = ButtonState.Released,
        [MouseButton.Button5] = ButtonState.Released,
    };

    public Texture2D Texture { get; set; }
    public static float WindowScale => Capy64.Instance.Scale;
    public const int MouseScrollDelta = 120;

    private Point mousePosition;
    private int vMouseScroll;
    private int hMouseScroll;

    private Modifiers keyboardMods = 0;
    private readonly HashSet<Keys> pressedKeys = new();

    private readonly Game _game;
    private readonly EventEmitter _eventEmitter;
    public InputManager(Game game, EventEmitter eventManager)

    {
        _game = game;
        _eventEmitter = eventManager;

        _game.Window.KeyDown += OnKeyDown;
        _game.Window.KeyUp += OnKeyUp;
        _game.Window.TextInput += OnTextInput;

        var mouseState = Mouse.GetState();
        vMouseScroll = mouseState.ScrollWheelValue;
        hMouseScroll = mouseState.HorizontalScrollWheelValue;
    }

    public void Update(bool IsActive)
    {
        UpdateMouse(Mouse.GetState(), IsActive);
        UpdateKeyboard(Keyboard.GetState(), IsActive);
        UpdateGamePad(GamePad.GetState(PlayerIndex.One), IsActive);
    }

    private void UpdateMouse(MouseState state, bool isActive)
    {
        if (!isActive)
            return;

        var rawPosition = state.Position - new Point(Capy64.Instance.Borders.Left, Capy64.Instance.Borders.Top);
        var pos = new Point((int)(rawPosition.X / WindowScale), (int)(rawPosition.Y / WindowScale)) + new Point(1, 1);

        if (pos.X < 1 || pos.Y < 1 || pos.X > Texture.Width || pos.Y > Texture.Height)
            return;

        if (pos != mousePosition)
        {
            mousePosition = pos;
            _eventEmitter.RaiseMouseMove(new()
            {
                Position = mousePosition,
                PressedButtons = mouseButtonStates
                    .Where(q => q.Value == ButtonState.Pressed)
                    .Select(q => (int)q.Key)
                    .ToArray()
            });
        }

        int vValue = 0;
        int hValue = 0;
        if (state.ScrollWheelValue != vMouseScroll)
        {
            var vDelta = vMouseScroll - state.ScrollWheelValue;
            vMouseScroll = state.ScrollWheelValue;
            vValue = vDelta / MouseScrollDelta;
        }

        if (state.HorizontalScrollWheelValue != hMouseScroll)
        {
            var hDelta = hMouseScroll - state.ScrollWheelValue;
            hMouseScroll = state.ScrollWheelValue;
            hValue = hDelta / MouseScrollDelta;
        }

        if (vValue != 0 || hValue != 0)
        {
            _eventEmitter.RaiseMouseWheelEvent(new()
            {
                Position = mousePosition,
                VerticalValue = vValue,
                HorizontalValue = hValue,
            });
        }

        // detect changes in mouse buttons
        if (state.LeftButton != mouseButtonStates[MouseButton.Left])
        {
            _eventEmitter.RaiseMouseButton(new()
            {
                Button = MouseButton.Left,
                State = state.LeftButton,
                Position = mousePosition,
            });
        }

        if (state.RightButton != mouseButtonStates[MouseButton.Right])
        {
            _eventEmitter.RaiseMouseButton(new()
            {
                Button = MouseButton.Right,
                State = state.RightButton,
                Position = mousePosition,
            });
        }

        if (state.MiddleButton != mouseButtonStates[MouseButton.Middle])
        {
            _eventEmitter.RaiseMouseButton(new()
            {
                Button = MouseButton.Middle,
                State = state.MiddleButton,
                Position = mousePosition,
            });
        }

        if (state.XButton1 != mouseButtonStates[MouseButton.Button4])
        {
            _eventEmitter.RaiseMouseButton(new()
            {
                Button = MouseButton.Button4,
                State = state.XButton1,
                Position = mousePosition,
            });
        }

        if (state.XButton2 != mouseButtonStates[MouseButton.Button5])
        {
            _eventEmitter.RaiseMouseButton(new()
            {
                Button = MouseButton.Button5,
                State = state.XButton2,
                Position = mousePosition,
            });
        }

        // update mouse states
        mouseButtonStates[MouseButton.Left] = state.LeftButton;
        mouseButtonStates[MouseButton.Right] = state.RightButton;
        mouseButtonStates[MouseButton.Middle] = state.MiddleButton;
        mouseButtonStates[MouseButton.Button4] = state.XButton1;
        mouseButtonStates[MouseButton.Button5] = state.XButton2;
    }

    private void UpdateKeyboard(KeyboardState state, bool isActive)
    {
        var keys = state.GetPressedKeys();

        if (keys.Contains(Keys.LeftControl))
            keyboardMods |= Modifiers.LCtrl;
        else
            keyboardMods &= ~Modifiers.LCtrl;

        if (keys.Contains(Keys.RightControl))
            keyboardMods |= Modifiers.RCtrl;
        else
            keyboardMods &= ~Modifiers.RCtrl;

        if (keys.Contains(Keys.LeftAlt))
            keyboardMods |= Modifiers.LAlt;
        else
            keyboardMods &= ~Modifiers.LAlt;

        if (keys.Contains(Keys.RightAlt))
            keyboardMods |= Modifiers.RAlt;
        else
            keyboardMods &= ~Modifiers.RAlt;

        if (keys.Contains(Keys.LeftShift))
            keyboardMods |= Modifiers.LShift;
        else
            keyboardMods &= ~Modifiers.LShift;

        if (keys.Contains(Keys.RightShift))
            keyboardMods |= Modifiers.RShift;
        else
            keyboardMods &= ~Modifiers.RShift;
    }

    private void OnKeyDown(object sender, InputKeyEventArgs e)
    {
        _eventEmitter.RaiseKeyDown(new()
        {
            KeyCode = (int)e.Key,
            KeyName = GetKeyName(e.Key),
            IsHeld = pressedKeys.Contains(e.Key),
            Key = e.Key,
            Mods = keyboardMods,
        });
        pressedKeys.Add(e.Key);
    }

    private void OnKeyUp(object sender, InputKeyEventArgs e)
    {
        _eventEmitter.RaiseKeyUp(new()
        {
            KeyCode = (int)e.Key,
            KeyName = GetKeyName(e.Key),
            Key = e.Key,
            Mods = keyboardMods,
        });
        pressedKeys.Remove(e.Key);
    }

    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (IgnoredTextInputKeys.Contains(e.Key))
        {
            return;
        }
        _eventEmitter.RaiseCharEvent(new()
        {
            Character = e.Character,
        });
    }

    public static string GetKeyName(Keys key)
    {
        var keyName = ToUnderscore(key.ToString());
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            keyName = keyName.TrimStart('d');
        }
        keyName = keyName.Replace("oem_", "");
        return keyName;
    }

    public static string ToUnderscore(string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }

    private GamePadState oldGamePadState = new();
    private void UpdateGamePad(GamePadState state, bool isActive)
    {
        if (!isActive)
            return;

        if (!state.IsConnected)
            return;

        var ob = oldGamePadState.Buttons;
        var b = state.Buttons;
        // ABXY
        if (ob.A != b.A)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.A,
                State = b.A,
            });
        }

        if (ob.B != b.B)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.B,
                State = b.B,
            });
        }

        if (ob.X != b.X)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.X,
                State = b.X,
            });
        }

        if (ob.Y != b.Y)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.Y,
                State = b.Y,
            });
        }

        // Back & Start
        if (ob.Back != b.Back)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.Back,
                State = b.Back,
            });
        }

        if (ob.Start != b.Start)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.Start,
                State = b.Start,
            });
        }

        // Shoulders
        if (ob.LeftShoulder != b.LeftShoulder)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.LeftShoulder,
                State = b.LeftShoulder,
            });
        }

        if (ob.RightShoulder != b.RightShoulder)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.RightShoulder,
                State = b.RightShoulder,
            });
        }

        // Sticks
        if (ob.LeftStick != b.LeftStick)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.LeftStick,
                State = b.LeftStick,
            });
        }

        if (ob.RightStick != b.RightStick)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.RightStick,
                State = b.RightStick,
            });
        }

        // BIG BUTTON
        if (ob.BigButton != b.BigButton)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.BigButton,
                State = b.BigButton,
            });
        }

        // D-PAD
        var od = oldGamePadState.DPad;
        var d = state.DPad;

        if (od.Left != d.Left)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.DPadLeft,
                State = d.Left,
            });
        }

        if (od.Right != d.Right)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.DPadRight,
                State = d.Right,
            });
        }

        if (od.Up != d.Up)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.DPadUp,
                State = d.Up,
            });
        }

        if (od.Down != d.Down)
        {
            _eventEmitter.RaiseGamePadButton(new()
            {
                Button = Buttons.DPadDown,
                State = d.Down,
            });
        }

        // triggers
        var ot = oldGamePadState.Triggers;
        var t = state.Triggers;

        if (ot.Left != t.Left)
        {

            _eventEmitter.RaiseGamePadTrigger(new()
            {
                Trigger = 1, // left
                Value = t.Left,
            });
        }

        if (ot.Right != t.Right)
        {

            _eventEmitter.RaiseGamePadTrigger(new()
            {
                Trigger = 2, // right
                Value = t.Right,
            });
        }

        // thumbsticks
        var os = oldGamePadState.ThumbSticks;
        var s = state.ThumbSticks;

        if (os.Left != s.Left)
        {

            _eventEmitter.RaiseGamePadThumbstick(new()
            {
                Stick = 1, // left
                Value = s.Left,
            });
        }

        if (os.Right != s.Right)
        {

            _eventEmitter.RaiseGamePadThumbstick(new()
            {
                Stick = 2, // right
                Value = s.Right,
            });
        }

        oldGamePadState = state;
    }
}
