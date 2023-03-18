// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
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
using static Capy64.Core.Audio;

namespace Capy64.Runtime.Libraries;

public class Audio : IComponent
{
    private const int queueLimit = 8;

    private static IGame _game;
    public Audio(IGame game)
    {
        _game = game;
        _game.EventEmitter.OnClose += OnClose;
    }

    private static readonly LuaRegister[] AudioLib = new LuaRegister[]
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

        if (_game.Audio.HQChannel.PendingBufferCount > queueLimit)
        {
            L.PushBoolean(false);
            L.PushString("queue is full");

            return 2;
        }

        try
        {
            var ts = _game.Audio.SubmitHQ(buffer);

            if (_game.Audio.HQChannel.State != SoundState.Playing)
                _game.Audio.HQChannel.Play();
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
        var timespan = TimeSpan.FromSeconds(time);

        var volume = (float)L.OptNumber(3, 1);
        volume = Math.Clamp(volume, 0, 1);

        var form = L.CheckOption(4, "sine", new string[]
        {
            "sine",
            "square",
            "triangle",
            "sawtooth",
            "noise",
            null,
        });

        var id = (int)L.OptInteger(5, -2);

        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.PushBoolean(false);
            return 1;
        }

        var buffer = GenerateWave(channel, (Waveform)form, freq, timespan, volume);

        try
        {
            var ts = _game.Audio.Submit(rid, buffer);

            if (channel.State != SoundState.Playing)
                channel.Play();

            L.PushBoolean(true);
        }
        catch (Exception ex)
        {
            L.Error(ex.Message);
        }

        return 1;
    }

    private static int L_Resume(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        channel.Resume();
        return 0;
    }
    private static int L_Pause(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        channel.Pause();
        return 0;
    }
    private static int L_Stop(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        channel.Stop();

        return 0;
    }

    private static int L_GetVolume(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        L.PushNumber(channel.Volume);

        return 1;
    }
    private static int L_SetVolume(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        var volume = (float)L.CheckNumber(2);
        volume = Math.Clamp(volume, 0, 1);

        channel.Volume = volume;

        return 0;
    }

    private static int L_Status(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var id = (int)L.CheckInteger(1);
        if (!_game.Audio.TryGetChannel(id, out var channel, out var rid))
        {
            L.ArgumentError(1, "channel id not found");
            return 0;
        }

        var status = channel.State switch
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
        foreach (var channel in _game.Audio.Channels)
        {
            channel.Stop();
        }
    }
}
