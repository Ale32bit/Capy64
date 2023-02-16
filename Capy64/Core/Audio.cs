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

using Microsoft.Xna.Framework.Audio;
using System;

namespace Capy64.Core;

public class Audio : IDisposable
{
    public enum Waveform
    {
        Sine,
        Square,
        Triangle,
        Sawtooth,
        Noise
    }

    public const int SampleRate = 16000;
    public const int HQSampleRate = 48000;
    public const AudioChannels AudioChannel = AudioChannels.Mono;
    public const int ChannelsCount = 5;
    public readonly DynamicSoundEffectInstance[] Channels = new DynamicSoundEffectInstance[ChannelsCount];
    private bool[] freeChannels = new bool[ChannelsCount];

    public readonly DynamicSoundEffectInstance HQChannel = new(HQSampleRate, AudioChannel);

    private static readonly Random rng = new();
    public Audio()
    {
        for (int i = 0; i < ChannelsCount; i++)
        {
            Channels[i] = new DynamicSoundEffectInstance(SampleRate, AudioChannel);
            freeChannels[i] = true;
            Channels[i].BufferNeeded += Audio_BufferNeeded;
        }

        HQChannel.BufferNeeded += HQChannel_BufferNeeded;
    }

    private void HQChannel_BufferNeeded(object sender, EventArgs e)
    {
        var pending = HQChannel.PendingBufferCount;
        Capy64.Instance.LuaRuntime.QueueEvent("audio_need", LK =>
        {
            LK.PushInteger(-1);
            LK.PushInteger(pending);
            return 2;
        });
    }

    private void Audio_BufferNeeded(object sender, EventArgs e)
    {
        for (int i = 0; i < ChannelsCount; i++)
        {
            if (Channels[i] == sender)
            {
                freeChannels[i] = true;
                var pending = Channels[i].PendingBufferCount;
                Capy64.Instance.LuaRuntime.QueueEvent("audio_need", LK =>
                {
                    LK.PushInteger(i);
                    LK.PushInteger(pending);
                    return 2;
                });
            }
        }

    }

    public int GetChannelId(int inp)
    {
        if (inp >= 0)
            return inp;

        if (inp == -1)
            return -1;

        if (inp == -2)
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                if (freeChannels[i])
                    return i;
            }
        }

        return -3;
    }

    public bool TryGetChannel(int id, out DynamicSoundEffectInstance channel, out int resolvedId)
    {
        resolvedId = GetChannelId(id);

        if (resolvedId >= 0)
            channel = Channels[resolvedId];
        else if (resolvedId == -1)
            channel = HQChannel;
        else
            channel = null;

        return channel != null;
    }

    public TimeSpan Submit(int id, byte[] buffer)
    {
        if (!TryGetChannel(id, out var channel, out var rId))
            return TimeSpan.Zero;

        channel.SubmitBuffer(buffer);
        freeChannels[rId] = false;
        return channel.GetSampleDuration(buffer.Length);
    }

    public TimeSpan SubmitHQ(byte[] buffer)
    {
        HQChannel.SubmitBuffer(buffer);
        return HQChannel.GetSampleDuration(buffer.Length);
    }

    public static byte[] GenerateWave(DynamicSoundEffectInstance channel, Waveform form, double frequency, TimeSpan time, float volume = 1f)
    {
        var size = channel.GetSampleSizeInBytes(time);
        var buffer = new byte[size];

        var step = 1d / SampleRate;
        double x = 0;
        for (int i = 0; i < size; i += 2)
        {
            var value = form switch
            {
                Waveform.Sine => GetSinePoint(frequency, x),
                Waveform.Square => GetSquarePoint(frequency, x),
                Waveform.Triangle => GetTrianglePoint(frequency, x),
                Waveform.Sawtooth => GetSawtoothPoint(frequency, x),
                Waveform.Noise => rng.NextDouble() * 2 - 1,
                _ => throw new NotImplementedException(),
            };

            value = Math.Clamp(value, -1, 1);
            var sample = (short)((value >= 0.0f ? value * short.MaxValue : value * short.MinValue * -1) * volume);
            if (!BitConverter.IsLittleEndian)
            {
                buffer[i] = (byte)(sample >> 8);
                buffer[i + 1] = (byte)sample;
            }
            else
            {
                buffer[i] = (byte)sample;
                buffer[i + 1] = (byte)(sample >> 8);
            }
            x += step;
        }

        return buffer;
    }

    public static double GetSinePoint(double frequency, double x)
    {
        return Math.Sin(x * 2 * Math.PI * frequency);
    }

    private static double GetSquarePoint(double frequency, double x)
    {
        double v = GetSinePoint(frequency, x);
        return v > 0 ? 1 : -1;
    }

    private static double GetTrianglePoint(double frequency, double x)
    {
        double v = 0;
        for (int k = 1; k <= 25; k++)
        {
            v += (Math.Pow(-1, k) / Math.Pow(2 * k - 1, 2))
                * Math.Sin(frequency * 2 * Math.PI * (2 * k - 1) * x);
        }
        return -(8 / Math.Pow(Math.PI, 2)) * v;
    }

    private static double GetSawtoothPoint(double frequency, double x)
    {
        double v = 0;
        for (int k = 1; k <= 50; k++)
        {
            v += (Math.Pow(-1, k) / k) * Math.Sin(frequency * 2 * Math.PI * k * x);
        }
        return -(2 / Math.PI) * v;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        for (int i = 0; i < ChannelsCount; i++)
        {
            Channels[i]?.Dispose();
        }
    }
}
