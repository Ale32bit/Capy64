using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Core;

public class Audio : IDisposable
{
    public const int SampleRate = 48000;
    public const AudioChannels Channels = AudioChannels.Mono;
    public readonly DynamicSoundEffectInstance Sound;

    private static readonly Random rng = new();
    public Audio()
    {
        GenerateSawtoothWave(440, 1);
        Sound = new DynamicSoundEffectInstance(SampleRate, Channels);
    }

    public TimeSpan Submit(byte[] buffer)
    {
        Sound.SubmitBuffer(buffer);
        return Sound.GetSampleDuration(buffer.Length);
    }

    public static byte[] GenerateSineWave(double frequency, double time, double volume = 1d)
    {
        var amplitude = 128 * volume;
        var timeStep = 1d / SampleRate;

        var buffer = new byte[(int)(SampleRate * time)];
        var ctime = 0d;
        for (int i = 0; i < buffer.Length; i++)
        {
            double angle = (Math.PI * frequency) * ctime;
            double factor = 0.5 * (Math.Sin(angle) + 1.0);
            buffer[i] = (byte)(amplitude * factor);
            ctime += timeStep;
        }
        return buffer;
    }

    public static byte[] GenerateSquareWave(double frequency, double time, double volume = 1d)
    {
        var amplitude = 128 * volume;
        var timeStep = 1d / SampleRate;

        var buffer = new byte[(int)(SampleRate * time)];
        var ctime = 0d;
        for (int i = 0; i < buffer.Length; i++)
        {
            double angle = (Math.PI * frequency) * ctime;
            double factor = Math.Sin(angle);
            buffer[i] = (byte)(factor >= 0 ? amplitude : 0);
            ctime += timeStep;
        }
        return buffer;
    }

    public static byte[] GenerateTriangleWave(double frequency, double time, double volume = 1d)
    {
        var amplitude = 128 * volume;
        var timeStep = 1d / SampleRate;
        frequency /= 2;

        var buffer = new byte[(int)(SampleRate * time)];
        var ctime = 0d;
        for (int i = 0; i < buffer.Length; i++)
        {
            var st = ctime * frequency - Math.Floor(ctime * frequency + 0.5);
            var x = Math.Abs(st) * 2.0f - 1.0f;
            buffer[i] = (byte)(amplitude * x);
            ctime += timeStep;
        }
        return buffer;
    }

    public static byte[] GenerateSawtoothWave(double frequency, double time, double volume = 1d)
    {
        var amplitude = 128 * volume;
        var timeStep = 1d / SampleRate;
        frequency /= 2;

        var buffer = new byte[(int)(SampleRate * time)];
        var ctime = 0d;
        for (int i = 0; i < buffer.Length; i++)
        {
            var x = ctime * frequency - Math.Floor(ctime * frequency + 0.5);
            buffer[i] = (byte)(amplitude * x);
            ctime += timeStep;
        }
        return buffer;
    }

    public static byte[] GenerateNoiseWave(double time, double volume = 1d)
    {
        var amplitude = 128 * volume;
        var timeStep = 1d / SampleRate;

        var buffer = new byte[(int)(SampleRate * time)];
        var ctime = 0d;
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)rng.Next(0, (int)amplitude);
            ctime += timeStep;
        }
        return buffer;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Sound.Dispose();
    }
}
