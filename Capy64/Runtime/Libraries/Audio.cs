using Capy64.API;
using KeraLua;
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
            var time = Core.Audio.Play(buffer, out var soundEffect);
            await Task.Delay(time - TimeSpan.FromMilliseconds(1000 / 60));
            _game.LuaRuntime.QueueEvent("audio_end", LK =>
            {
                soundEffect.Dispose();

                return 0;
            });

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static int L_Play(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = L.CheckBuffer(1);

        if (queue.Count > queueLimit)
        {
            L.PushBoolean(false);
            L.PushString("queue is full");

            return 2;
        }

        queue.Enqueue(buffer);

        if (!isProcessing)
        {
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

        return 0;
    }
}
