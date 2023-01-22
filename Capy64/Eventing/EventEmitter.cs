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

    // Functional events
    public event EventHandler<TickEvent> OnTick;
    public event EventHandler OnInit;
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
