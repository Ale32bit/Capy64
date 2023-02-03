using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Core;

public class Audio
{
    public const int SampleRate = 48000;
    public static TimeSpan Play(byte[] buffer, out SoundEffect soundEffect)
    {
        soundEffect = new SoundEffect(buffer, SampleRate, AudioChannels.Stereo);

        soundEffect.Play(1, 0, 0);

        return soundEffect.Duration;
    }
}
