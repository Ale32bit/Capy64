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
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

    public const int SampleRate = 48000;
    public const AudioChannels Channels = AudioChannels.Mono;
    public readonly DynamicSoundEffectInstance Sound;

    private static readonly Random rng = new();
    public Audio()
    {
        Sound = new DynamicSoundEffectInstance(SampleRate, Channels);
    }

    public TimeSpan Submit(byte[] buffer)
    {
        Sound.SubmitBuffer(buffer);
        return Sound.GetSampleDuration(buffer.Length);
    }

    public byte[] GenerateWave(Waveform form, double frequency, TimeSpan time)
    {
        var size = Sound.GetSampleSizeInBytes(time);
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
            Console.WriteLine(value);
            value = Math.Clamp(value, -1, 1);
            var sample = (short)(value >= 0.0f ? value * short.MaxValue : value * short.MinValue * -1);
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
        Sound.Dispose();
    }
}
