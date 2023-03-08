using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace Sounds
{ 
    public class LongBeep
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