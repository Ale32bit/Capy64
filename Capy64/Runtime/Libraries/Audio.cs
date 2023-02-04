using Capy64.API;
using Capy64.Core;
using KeraLua;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Capy64.Runtime.Libraries;

public class Audio : IPlugin
{
    private const int queueLimit = 8;

    private static ConcurrentQueue<byte[]> queue = new();
    private static bool isProcessing = false;
    private static IGame _game;
    public Audio(IGame game)
    {
        _game = game;
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

    private static async Task PlayAudio(byte[] buffer)
    {
        try
        {
            var time = _game.Audio.Submit(buffer);
            if (_game.Audio.Sound.State != Microsoft.Xna.Framework.Audio.SoundState.Playing)
            {
                _game.Audio.Sound.Play();
            }
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void ProcessAudio()
    {
        if (isProcessing)
            return;

        isProcessing = true;
        Task.Run(async () =>
        {
            while (queue.TryDequeue(out var buffer))
            {
                await PlayAudio(buffer);
            }
            isProcessing = false;
        });
    }

    private static int L_Play(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = L.CheckBuffer(1);

        if (_game.Audio.Sound.PendingBufferCount > queueLimit)
        {
            L.PushBoolean(false);
            L.PushString("queue is full");

            return 2;
        }

        queue.Enqueue(buffer);

        ProcessAudio();

        return 0;
    }

    private static int L_Beep(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var freq = L.CheckNumber(1);
        var time = L.OptNumber(2, 1);
        var volume = L.OptNumber(3, 1);
        Math.Clamp(volume, 0, 1);

        var sine = Core.Audio.GenerateSquareWave(freq, time, volume);

        queue.Enqueue(sine);

        ProcessAudio();

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
        queue.Clear();
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
}
