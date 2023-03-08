using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace Sounds
{ 
    public class Silence
    {
        public static float[] GenerateSilence(double duration, int sampleRate)
        {
            // Calculate the number of samples based on the duration and sample rate
            int numSamples = (int)(duration * sampleRate);

            // Create an array of zeros with the same length as the number of samples
            float[] buffer = new float[numSamples];

            // Return the array of zeros
            return buffer;
        }
    }
}