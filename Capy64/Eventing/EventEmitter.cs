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

using Capy64.Eventing.Events;
using Microsoft.Xna.Framework.Input;
using System;


namespace Capy64.Eventing;

public class EventEmitter
{
    // Mouse events
    public event EventHandler<MouseButtonEvent> OnMouseDown;
    public event EventHandler<MouseButtonEvent> OnMouseUp;
    public event EventHandler<MouseMoveEvent> OnMouseMove;
    public event EventHandler<MouseWheelEvent> OnMouseWheel;

    // Keyboard events
    public event EventHandler<KeyEvent> OnKeyDown;
    public event EventHandler<KeyEvent> OnKeyUp;
    public event EventHandler<CharEvent> OnChar;

    // GamePad events
    public event EventHandler<GamePadButtonEvent> OnGamePadButton;
    public event EventHandler<GamePadTriggerEvent> OnGamePadTrigger;
    public event EventHandler<GamePadThumbstickEvent> OnGamePadThumbstick;

    // Functional events
    public event EventHandler<TickEvent> OnTick;
    public event EventHandler OnInit;
    public event EventHandler OnClose;
    public event EventHandler OnScreenSizeChange;
    public event EventHandler<OverlayEvent> OnOverlay;


    public void RaiseMouseMove(MouseMoveEvent ev)
    {
        if (OnMouseMove is not null)
        {
            OnMouseMove(this, ev);
        }
    }

    public void RaiseMouseButton(MouseButtonEvent ev)
    {
        if (ev.State == ButtonState.Released)
        {
            if (OnMouseUp is not null)
            {
                OnMouseUp(this, ev);
            }
        }
        else
        {
            if (OnMouseDown is not null)
            {
                OnMouseDown(this, ev);
            }
        }
    }

    public void RaiseMouseWheelEvent(MouseWheelEvent ev)
    {
        if (OnMouseWheel is not null)
        {
            OnMouseWheel(this, ev);
        }
    }


    public void RaiseKeyDown(KeyEvent ev)
    {
        if (OnKeyDown is not null)
        {
            OnKeyDown(this, ev);
        }
    }

    public void RaiseKeyUp(KeyEvent ev)
    {
        if (OnKeyUp is not null)
        {
            OnKeyUp(this, ev);
        }
    }

    public void RaiseCharEvent(CharEvent ev)
    {
        if (OnChar is not null)
        {
            OnChar(this, ev);
        }
    }

    public void RaiseGamePadButton(GamePadButtonEvent ev)
    {
        if (OnGamePadButton is not null)
        {
            OnGamePadButton(this, ev);
        }
    }

    public void RaiseGamePadTrigger(GamePadTriggerEvent ev)
    {
        if (OnGamePadTrigger is not null)
        {
            OnGamePadTrigger(this, ev);
        }
    }

    public void RaiseGamePadThumbstick(GamePadThumbstickEvent ev)
    {
        if (OnGamePadThumbstick is not null)
        {
            OnGamePadThumbstick(this, ev);
        }
    }

    public void RaiseTick(TickEvent ev)
    {
        if (OnTick is not null)
        {
            OnTick(this, ev);
        }
    }

    public void RaiseInit()
    {
        if (OnInit is not null)
        {
            OnInit(this, EventArgs.Empty);
        }
    }

    public void RaiseClose()
    {
        if (OnClose is not null)
        {
            OnClose(this, EventArgs.Empty);
        }
    }

    public void RaiseScreenSizeChange()
    {
        if (OnScreenSizeChange is not null)
        {
            OnScreenSizeChange(this, EventArgs.Empty);
        }
    }

    public void RaiseOverlay(OverlayEvent ev)
    {
        if(OnOverlay is not null) {
            OnOverlay(this, ev);
        }
    }
}
