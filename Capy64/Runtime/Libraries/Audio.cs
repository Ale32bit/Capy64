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

using Capy64.API;
using KeraLua;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Threading.Tasks;
using static Capy64.Core.Audio;

namespace Capy64.Runtime.Libraries;

public class Audio : IPlugin
{
    private const int queueLimit = 8;

    private static IGame _game;
    public Audio(IGame game)
    {
        _game = game;
        _game.EventEmitter.OnClose += OnClose;
    }

    private static LuaRegister[] AudioLib = new LuaRegister[]
    {
        new()
        {
            name = "play",
            function = L_Play,
        },
        new()
        {
            name = "beep",
            function = L_Beep,
        },
        new()
        {
            name = "resume",
            function = L_Resume,
        },
        new()
        {
            name = "pause",
            function = L_Pause,
        },
        new()
        {
            name = "stop",
            function = L_Stop,
        },
        new()
        {
            name = "getVolume",
            function = L_GetVolume,
        },
        new()
        {
            name = "setVolume",
            function = L_SetVolume,
        },
        new()
        {
            name = "status",
            function = L_Status,
        },
        new(),
    };

    public void LuaInit(Lua L)
    {
        L.RequireF("audio", OpenLib, false);
    }

    private static int OpenLib(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(AudioLib);
        return 1;
    }

    private static async Task DelayEmit(TimeSpan time)
    {
        var waitTime = time - TimeSpan.FromMilliseconds(1000 / 60);
        if (waitTime.TotalMilliseconds < 0)
            waitTime = time;
        await Task.Delay(waitTime);
        _game.LuaRuntime.QueueEvent("audio_end", LK =>
        {
            LK.PushInteger(_game.Audio.Sound.PendingBufferCount);
            return 1;
        });
    }

    private static int L_Play(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        byte[] buffer;

        if (L.IsString(1))
        {
            buffer = L.CheckBuffer(1);
        }
        else
        {
            L.CheckType(1, LuaType.Table);
            var len = L.RawLen(1);
            buffer = new byte[len];
            for (int i = 1; i <= len; i++)
            {
                L.GetInteger(1, i);
                var value = L.CheckInteger(-1);
                buffer[i - 1] = (byte)value;
                L.Pop(1);
            }
        }

        if (_game.Audio.Sound.PendingBufferCount > queueLimit)
        {
            L.PushBoolean(false);
            L.PushString("queue is full");

            return 2;
        }

        try
        {
            var ts = _game.Audio.Submit(buffer);
            DelayEmit(ts);

            if (_game.Audio.Sound.State != SoundState.Playing)
                _game.Audio.Sound.Play();
        }
        catch (Exception ex)
        {
            L.Error(ex.Message);
        }

        return 0;
    }

    private static int L_Beep(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var freq = L.OptNumber(1, 440);
        var time = L.OptNumber(2, 1);
        var volume = L.OptNumber(3, 1);
        volume = Math.Clamp(volume, 0, 1);

        var timespan = TimeSpan.FromSeconds(time);

        var form = L.CheckOption(4, "sine", new string[]
        {
            "sine",
            "square",
            "triangle",
            "sawtooth",
            "noise",
            null,
        });

        var buffer = _game.Audio.GenerateWave((Waveform)form, freq, timespan);

        try
        {
            var ts = _game.Audio.Submit(buffer);
            DelayEmit(ts);

            if (_game.Audio.Sound.State != SoundState.Playing)
                _game.Audio.Sound.Play();
        }
        catch (Exception ex)
        {
            L.Error(ex.Message);
        }

        return 0;
    }

    private static int L_Resume(IntPtr state)
    {
        _game.Audio.Sound.Resume();
        return 0;
    }
    private static int L_Pause(IntPtr state)
    {
        _game.Audio.Sound.Pause();
        return 0;
    }
    private static int L_Stop(IntPtr state)
    {
        _game.Audio.Sound.Stop();

        return 0;
    }

    private static int L_GetVolume(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushNumber(_game.Audio.Sound.Volume);

        return 1;
    }
    private static int L_SetVolume(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var volume = (float)L.CheckNumber(1);
        volume = Math.Clamp(volume, 0, 1);

        _game.Audio.Sound.Volume = volume;

        return 0;
    }

    private static int L_Status(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var status = _game.Audio.Sound.State switch
        {
            SoundState.Playing => "playing",
            SoundState.Paused => "paused",
            SoundState.Stopped => "stopped",
            _ => "unknown",
        };

        L.PushString(status);

        return 1;
    }

    private void OnClose(object sender, EventArgs e)
    {
        _game.Audio.Sound.Stop(true);
    }
}
