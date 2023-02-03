using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Core;

public class Audio
{
    public const int SampleRate = 48000;
    public static TimeSpan Play(byte[] buffer)
    {
        var soundEffect = new SoundEffect(buffer, SampleRate, AudioChannels.Mono);

        soundEffect.Play();

        return soundEffect.Duration;
    }
}
