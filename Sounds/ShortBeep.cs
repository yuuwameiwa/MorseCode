using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorseCode.Sounds
{
    public class ShortBeep
    {
        public static float[] GenerateTone(float frequency, double duration, int sampleRate)
        {
            // Create a sine wave generator with the specified frequency
            SignalGenerator generator = new SignalGenerator(sampleRate, 1);
            generator.Frequency = frequency;

            // Generate the audio data for the specified duration
            int numSamples = (int)(sampleRate * duration);
            float[] buffer = new float[numSamples];
            generator.Read(buffer, 0, numSamples);

            return buffer;
        }
    }
}
