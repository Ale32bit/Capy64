using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Threading.Channels;

namespace Capy64.Core;

public class Audio
{
    public const int SampleRate = 48000;
    public const int SamplesPerBuffer = 3000;
    const int bytesPerSample = 2;

    private readonly DynamicSoundEffectInstance _instance;
    private double _time = 0.0;
    public Audio()
    {
        _instance = new(SampleRate, AudioChannels.Mono);
    }

    public void test()
    {
        var workingBuffer = new float[SamplesPerBuffer];
        FillWorkingBuffer(workingBuffer);
        var buffer = DivideBuffer(workingBuffer);
        _instance.SubmitBuffer(buffer);
    }

    private byte[] DivideBuffer(float[] from)
    {
        var outBuffer = new byte[SamplesPerBuffer * bytesPerSample];

        for (int i = 0; i < from.Length; i++) {
            var floatSample = MathHelper.Clamp(from[i], -1.0f, 1.0f);
            var shortSample = (short)(floatSample * short.MaxValue);

            int index = i * bytesPerSample + bytesPerSample;

            if (!BitConverter.IsLittleEndian)
            {
                outBuffer[index] = (byte)(shortSample >> 8);
                outBuffer[index + 1] = (byte)shortSample;
            }
            else
            {
                outBuffer[index] = (byte)shortSample;
                outBuffer[index + 1] = (byte)(shortSample >> 8);
            }
        }

        return outBuffer;
    }

    public static double SineWave(double time, double frequency)
    {
        return Math.Sin(time * 2 * Math.PI * frequency);
    }

    private void FillWorkingBuffer(float[] buffer)
    {
        for (int i = 0; i < SamplesPerBuffer; i++)
        {
            // Here is where you sample your wave function
            buffer[i] = (float)SineWave(_time, 440); // Left Channel

            // Advance time passed since beginning
            // Since the amount of samples in a second equals the chosen SampleRate
            // Then each sample should advance the time by 1 / SampleRate
            _time += 1.0 / SampleRate;
        }
    }

}
