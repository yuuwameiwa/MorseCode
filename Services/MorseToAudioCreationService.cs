using MorseCode.Models;

namespace MorseCode.Services
{
    public class MorseToAudioCreationService
    {
        public string[] alphabetData { get; set; }
        public string sData { get; set; }
        public int wpm { get; set; }
        public int freq { get; set; }
        public string outPath { get; set; }
        public double volume { get; set; }

        public MorseToAudioCreationService(string[] alpData, string stringData, string oPath)
        {
            this.alphabetData = alpData;
            this.sData = stringData;
            this.wpm = 25;
            this.outPath = oPath;
            this.freq = 800;
            this.volume = 0.5;
        }

        public (WaveHeaderChunk, WaveFormatChunk, WaveDataChunk) GenerateAudio()
        {
            float unit = 60 / (50 * (float)wpm);

            string[] wrstr = sData.Split(' ');
            List<Signal> sig = new List<Signal>();

            for (int i = 0; i < wrstr.Length; i++)
            {
                string str = wrstr[i];
                for (int j = 0; j < str.Length; j++)
                {
                    char symbol = str[j];

                    if (symbol == '0' )
                    {
                        sig.Add(new Signal(SignalType.Full, 1));
                        sig.Add(new Signal(SignalType.Empty, 1));
                    }
                    else if (symbol == '1' )
                    {
                        sig.Add(new Signal(SignalType.Full, 3));
                        sig.Add(new Signal(SignalType.Empty, 1));
                    }

                    /*foreach (string entry in alphabetData)
                    {
                        if (entry.Trim() == "") continue;

                        string[] pts = entry.Split(' ');

                        if (pts[1] == symbol.ToString())
                        {
                            for (int x = 0; x < pts[0].Length; x++)
                            {
                                char c = pts[0][x];
                                if (c == '0')
                                {
                                    sig.Add(new Signal(SignalType.Full, 1));
                                }
                                else if (c == '1')
                                {
                                    sig.Add(new Signal(SignalType.Full, 3));
                                }

                                if (x != pts[0].Length - 1)
                                {
                                    sig.Add(new Signal(SignalType.Empty, 1));
                                }
                            }
                        }
                    }*/

                    sig.Add(new Signal(SignalType.Empty, 2));
                }

                sig.Add(new Signal(SignalType.Empty, 5));
            }

            //sig.Add(new Signal(SignalType.Empty, 1)); // add 1 unit silence for errors

            float unitduration = sig.Sum(x => x.Duration);
            float secduration = unit * unitduration;

            WaveHeaderChunk header = new WaveHeaderChunk();
            WaveFormatChunk format = new WaveFormatChunk();
            WaveDataChunk data = new WaveDataChunk();

            uint numSamples = (uint)(format.dwSamplesPerSec * format.wChannels * secduration);

            data.shortArray = new short[numSamples];

            uint cursample = 0;
            foreach (Signal s in sig)
            {
                //float cfreq = s.Type == SignalType.Full ? freq : 600;
                double sindata = (Math.PI * 2 * freq) / (format.dwSamplesPerSec * format.wChannels);

                //float vol = 0.5f;
                float amplitude = (int)s.Type * (32767f * (float)volume);

                for (uint i = 0; i < (uint)((float)(s.Duration * unit) * (float)format.dwSamplesPerSec); i++)
                {
                    if (cursample <= numSamples - 1)
                    {
                        for (int channel = 0; channel < format.wChannels; channel++)
                        {
                            data.shortArray[cursample + channel] = Convert.ToInt16(amplitude * Math.Sin(sindata * i));
                        }

                        cursample++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            data.dwChunkSize = (uint)(data.shortArray.Length * (format.wBitsPerSample / 8));

            return (header, format, data);
        }

        public void WriteWavefile(WaveHeaderChunk header, WaveFormatChunk format, WaveDataChunk data)
        {
            FileStream fileStream = new FileStream(outPath, FileMode.Create);

            BinaryWriter writer = new BinaryWriter(fileStream);

            // Write the header
            writer.Write(header.sGroupID.ToCharArray());
            writer.Write(header.dwFileLength);
            writer.Write(header.sRiffType.ToCharArray());

            // Write the format chunk
            writer.Write(format.sChunkID.ToCharArray());
            writer.Write(format.dwChunkSize);
            writer.Write(format.wFormatTag);
            writer.Write(format.wChannels);
            writer.Write(format.dwSamplesPerSec);
            writer.Write(format.dwAvgBytesPerSec);
            writer.Write(format.wBlockAlign);
            writer.Write(format.wBitsPerSample);

            // Write the data chunk
            writer.Write(data.sChunkID.ToCharArray());
            writer.Write(data.dwChunkSize);
            foreach (short dataPoint in data.shortArray)
            {
                writer.Write(dataPoint);
            }

            writer.Seek(4, SeekOrigin.Begin);
            uint filesize = (uint)writer.BaseStream.Length;
            writer.Write(filesize - 8);

            // Clean up
            writer.Close();
            fileStream.Close();
        }
    }
}
