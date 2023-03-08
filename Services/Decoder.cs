using MorseCode.Models;
using NAudio.SoundFont;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorseCode.Services
{
    internal class Decoder
    {
        public static Decoder decoder = new Decoder();

        public static WaveInEvent deviceInput = new WaveInEvent();
        public static int devSampleRate = 48000;

        public static Timer decTimer = null;
        public static int timerInterval = 3000; // hmm
        public static List<Argument> argList = new List<Argument>();
        public static bool debug = false;

        public string[] alphabetData = File.ReadAllLines("symbols.txt");

        public const int stepsize = 100;
        public const int fftsize = 512;

        public async Task DecodeAsync(string path)
        {
            string[] pts = path.Split(':');
            if (pts[0] == "device" && pts.Length == 2)
            {
                if (pts[1] == "list")
                {
                    for (int i = 0; i < WaveInEvent.DeviceCount; i++)
                    {
                        Console.WriteLine("{0} {1}", i, WaveInEvent.GetCapabilities(i).ProductName);
                    }

                    Environment.Exit(0);
                }

                int deviceIndex;
                if (!int.TryParse(pts[1], out deviceIndex))
                {
                    Console.WriteLine("Device index should be an integer.");
                    Environment.Exit(-1);
                }

                deviceInput = new WaveInEvent();
                deviceInput.DeviceNumber = deviceIndex;
                deviceInput.WaveFormat = new WaveFormat(rate: devSampleRate, bits: 16, channels: 1);
                deviceInput.DataAvailable += OnDeviceInput;
                deviceInput.BufferMilliseconds = 20;
                deviceInput.StartRecording();

                decTimer = new Timer(TimerCallback, null, timerInterval, Timeout.Infinite);

                await Task.Delay(-1);
            }
            else
            {
                (int sampleRate, double[] data) = AudioFile.ReadMono(path);

                double[][] fftdata = decoder.Transform(data, sampleRate);
                List<Signal> signaldata = decoder.ParseFFTData(fftdata, sampleRate);

                string decodedSignal = decoder.DecodeSignal(signaldata);

                Console.WriteLine(decodedSignal);
            }
        }

        public string DecodeSignal(List<Signal> msignal)
        {
            int maxFullDur = msignal.Max((s) =>
            {
                return s.Type == SignalType.Full ? s.Duration : 0;
            });

            int minFullDur = msignal.Min((s) =>
            {
                return s.Type == SignalType.Full ? s.Duration : maxFullDur;
            });

            int maxEmpDur = msignal.Max((s) =>
            {
                return s.Type == SignalType.Empty ? s.Duration : 0;
            });

            int minEmpDur = msignal.Min((s) =>
            {
                return s.Type == SignalType.Empty ? s.Duration : maxEmpDur;
            });

            if (minEmpDur == 1) // i'm not very sure about this
            {
                if (debug) Console.WriteLine("WPM is high, trying to adjust the spacing... (Result can be affected)");

                //minEmpDur++;

                /* getting the next min. space duration */
                int min = msignal.Min((s) =>
                {
                    if (s.Type == SignalType.Empty && s.Duration != minEmpDur)
                    {
                        return s.Duration;
                    }
                    else
                    {
                        return maxEmpDur;
                    }
                });

                minEmpDur = min;
            }

            if (minFullDur == 1)
            {
                if (debug) Console.WriteLine("Error signal or very high (60<) WPM. Trying to fix... (Result can be affected)");

                /* getting the next min. space dur. */
                int min = msignal.Min((s) =>
                {
                    if (s.Type == SignalType.Full && s.Duration != minFullDur)
                    {
                        return s.Duration;
                    }
                    else
                    {
                        return maxFullDur;
                    }
                });

                minFullDur = min;
            }

            List<List<Signal>> words = new List<List<Signal>>();

            if (maxEmpDur > minEmpDur * 5)
            {
                List<Signal> cList = new List<Signal>();
                foreach (Signal sig in msignal)
                {
                    if (sig.Type == SignalType.Empty && sig.Duration > minEmpDur * 5)
                    {
                        words.Add(cList.ToList());

                        cList.Clear();
                    }
                    else
                    {
                        cList.Add(sig);
                    }
                }

                words.Add(cList);
            }
            else
            {
                words.Add(msignal);
            }

            List<List<string>> fdataLst = new List<List<string>>();
            foreach (List<Signal> word in words)
            {
                maxEmpDur = word.Max((s) =>
                {
                    return s.Type == SignalType.Empty ? s.Duration : 0;
                });

                double fullTolerance = (maxFullDur - minFullDur) / 4;
                double empTolerance = (maxEmpDur - minEmpDur) / 4;

                /* parsing letters/symbols */
                List<List<Signal>> symbols = new List<List<Signal>>();
                if (maxEmpDur > minEmpDur * 2)
                {
                    List<Signal> cList = new List<Signal>();
                    foreach (Signal sig in word)
                    {
                        if (sig.Type == SignalType.Empty && sig.Duration > minEmpDur * 2)
                        {
                            symbols.Add(cList.ToList());

                            cList.Clear();
                        }
                        else
                        {
                            cList.Add(sig);
                        }
                    }

                    symbols.Add(cList);
                }
                else
                {
                    symbols.Add(word);
                }

                /* converting word to str */
                List<string> wordLstData = new List<string>();
                foreach (List<Signal> symbol in symbols)
                {
                    string symStr = "";
                    foreach (Signal s in symbol)
                    {
                        if (s.Type == SignalType.Full)
                        {
                            if (maxFullDur.InBounds(s.Duration, fullTolerance))
                            {
                                symStr += "1";
                            }

                            if (minFullDur.InBounds(s.Duration, fullTolerance))
                            {
                                symStr += "0";
                            }
                        }
                    }

                    wordLstData.Add(symStr);
                }

                fdataLst.Add(wordLstData);
            }

            /*List<string> dbgstr = new List<string>();
            foreach (var d in fdataLst)
            {
                dbgstr.Add(string.Join(" ", d));
            }
            string finalData = string.Join(" / ", dbgstr);

            Console.WriteLine(finalData);*/

            List<string> outstrLst = new List<string>();

            foreach (List<string> word in fdataLst)
            {
                string wdata = "";
                foreach (string symbol in word)
                {
                    foreach (string entry in alphabetData)
                    {
                        if (entry.Trim() == "") continue;

                        string[] pts = entry.Split(' ');
                        if (pts[0] == symbol)
                        {
                            wdata += pts[1];
                            break;
                        }
                    }
                }

                outstrLst.Add(wdata);
            }

            return string.Join(" ", outstrLst);
        }

        private double mFreq(double[] arr)
        {
            int maxcount = 0;
            double max_freq = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                int count = 0;
                for (int j = 0; j < arr.Length; j++)
                {
                    if (arr[i] == arr[j])
                    {
                        count++;
                    }
                }

                if (count > maxcount)
                {
                    maxcount = count;
                    max_freq = arr[i];
                }
            }

            return max_freq;
        }

        public double[][] Transform(double[] data, int sampleRate)
        {
            int stepsize = 100;
            int fftsize = 512;

            int nyquist = sampleRate / 2;
            int height = fftsize / 2;

            int fftcn = (data.Length - fftsize) / stepsize;
            double[][] fftdB = new double[fftcn][];

            var window = new FftSharp.Windows.Hanning().Create(fftsize);

            Parallel.For(0, fftcn, newFftIndex =>
            {
                double[] buffer = new double[fftsize];
                long sourceIndex = newFftIndex * stepsize;
                for (int i = 0; i < fftsize; i++)
                    buffer[i] = data[sourceIndex + i] * window[i];

                fftdB[newFftIndex] = FftSharp.Transform.FFTpower(buffer);
            });

            return fftdB;
        }

        public List<Signal> ParseFFTData(double[][] fftdB, int sampleRate)
        {
            List<double> freqData = new List<double>();
            List<double> dBData = new List<double>();

            double filterdB = 50; // min. dB for peak frequency

            int height = fftsize / 2; // for the frequency map

            double[] freqfft = FftSharp.Transform.FFTfreq(sampleRate, height);
            for (int i = 0; i < fftdB.Length; i++)
            {
                double[] nfft = fftdB[i];
                double m = 0;
                int mindex = 0;

                for (int j = 0; j < nfft.Length; j++)
                {
                    double nf = nfft[j];
                    if (nf > m && nf > filterdB)
                    {
                        m = nf;
                        mindex = j;
                    }
                }

                double pfreq = freqfft[mindex];

                freqData.Add(pfreq);
                dBData.Add(m);
            }

            List<Signal> msignal = new List<Signal>();

            int spaceCount = 0;
            int signalCount = 0;
            double signalFreq = 0;
            bool started = false;

            List<double> zeroRemovedF = new List<double>();
            zeroRemovedF = zeroRemovedF.Concat(freqData).ToList();
            zeroRemovedF.RemoveAll(i => (i == 0));
            double avfreq = mFreq(zeroRemovedF.ToArray());

            double avdB = dBData.Max();

            for (int j = 0; j < dBData.Count; j++)
            {
                if (dBData[j] == 0 || !avdB.InBounds(dBData[j], 10) || !avfreq.InBounds(freqData[j], 20))
                {
                    if (started)
                    {
                        if (signalCount != 0)
                        {
                            msignal.Add(new Signal(SignalType.Full, signalCount, signalFreq));
                        }

                        signalCount = 0;
                        signalFreq = 0;
                        spaceCount++;
                    }
                }
                else
                {
                    if (!started) started = true;

                    if (spaceCount != 0)
                    {
                        msignal.Add(new Signal(SignalType.Empty, spaceCount, 0));
                    }

                    signalFreq = freqData[j];
                    spaceCount = 0;
                    signalCount++;
                }
            }

            return msignal;
        }

        List<double> buffer = new List<double>();
        private void OnDeviceInput(object sender, WaveInEventArgs args)
        {
            int bytesPerSample = deviceInput.WaveFormat.BitsPerSample / 8;
            int samplesRecorded = args.BytesRecorded / bytesPerSample;

            for (int i = 0; i < samplesRecorded; i++)
                buffer.Add(BitConverter.ToInt16(args.Buffer, i * bytesPerSample));
        }

        private void TimerCallback(object state)
        {
            Thread decodeThread = new Thread(DecodeBuffer);
            decodeThread.Start();

            decTimer.Change(timerInterval, Timeout.Infinite);
        }

        string cbuf = "";
        //string cStr = "";
        private void DecodeBuffer()
        {
            double[][] fftdata = decoder.Transform(buffer.ToArray(), devSampleRate);
            List<Signal> signaldata = decoder.ParseFFTData(fftdata, devSampleRate);
            if (signaldata.Count == 0)
            {
                buffer.Clear();
                return;
            }

            string output = decoder.DecodeSignal(signaldata).Trim();

            if (output != "")
            {
                Console.WriteLine(cbuf + output);
                buffer.Clear();

                cbuf += output;
            }
        }
    }

    internal class Argument
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public Argument(string n, string v)
        {
            this.Name = n;
            this.Value = v;
        }
    }

    internal static class Extensions
    {
        public static bool InRange(this double obj, double f, double l)
        {
            return f <= obj && obj <= l;
        }

        public static bool InRange(this int obj, double f, double l)
        {
            return f <= obj && obj <= l;
        }

        public static bool InBounds(this double obj, double x, double n)
        {
            return obj - n <= x && x <= obj + n;
        }

        public static bool InBounds(this int obj, double x, double n)
        {
            return obj - n <= x && x <= obj + n;
        }

        public static List<double> getPlVal(this List<Signal> lst)
        {
            List<double> sgn = new List<double>();
            foreach (Signal s in lst)
            {
                sgn = sgn.Concat(Enumerable.Repeat(s.Frequency, s.Duration)).ToList();
            }

            return sgn;
        }
    }

    public static class AudioFile
    {
        /* https://github.com/swharden/Spectrogram/blob/main/src/Spectrogram/WavFile.cs */

        private static (string id, uint length) ChunkInfo(BinaryReader br, long position)
        {
            br.BaseStream.Seek(position, SeekOrigin.Begin);
            string chunkID = new string(br.ReadChars(4));
            uint chunkBytes = br.ReadUInt32();
            return (chunkID, chunkBytes);
        }

        public static (int sampleRate, double[] L) ReadMono(string filePath)
        {
            (int sampleRate, double[] L, _) = ReadStereo(filePath);
            return (sampleRate, L);
        }

        public static (int sampleRate, double[] L, double[] R) ReadStereo(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                var (id, length) = ChunkInfo(br, 0);
                if (id != "RIFF")
                    throw new InvalidOperationException($"Unsupported WAV format (first chunk ID was '{id}', not 'RIFF')");

                var fmtChunk = ChunkInfo(br, 12);
                if (fmtChunk.id != "fmt ")
                    throw new InvalidOperationException($"Unsupported WAV format (first chunk ID was '{fmtChunk.id}', not 'fmt ')");
                if (fmtChunk.length != 16)
                    throw new InvalidOperationException($"Unsupported WAV format (expect 16 byte 'fmt' chunk, got {fmtChunk.length} bytes)");

                int audioFormat = br.ReadUInt16();
                if (audioFormat != 1)
                    throw new NotImplementedException("Unsupported WAV format (audio format must be 1, indicating uncompressed PCM data)");

                int channelCount = br.ReadUInt16();
                if (channelCount < 0 || channelCount > 2)
                    throw new NotImplementedException($"Unsupported WAV format (must be 1 or 2 channel, file has {channelCount})");

                int sampleRate = (int)br.ReadUInt32();

                int byteRate = (int)br.ReadUInt32();

                ushort blockSize = br.ReadUInt16();
                //Console.WriteLine($"block size: {blockSize} bytes per sample");

                ushort bitsPerSample = br.ReadUInt16();
                //Console.WriteLine($"resolution: {bitsPerSample}-bit");
                if (bitsPerSample != 16)
                    throw new NotImplementedException("Only 16-bit WAV files are supported");

                long nextChunkPosition = 36;
                int maximumChunkNumber = 42;
                long firstDataByte = 0;
                long dataByteCount = 0;
                for (int i = 0; i < maximumChunkNumber; i++)
                {
                    var chunk = ChunkInfo(br, nextChunkPosition);
                    if (chunk.id == "data")
                    {
                        firstDataByte = nextChunkPosition + 8;
                        dataByteCount = chunk.length;
                        break;
                    }
                    nextChunkPosition += chunk.length + 8;
                }
                if (firstDataByte == 0 || dataByteCount == 0)
                    throw new InvalidOperationException("Unsupported WAV format (no 'data' chunk found)");

                long sampleCount = dataByteCount / blockSize;

                double[] L = new double[sampleCount];
                double[] R = new double[sampleCount];

                if (channelCount == 1)
                {
                    for (int i = 0; i < sampleCount; i++)
                    {
                        L[i] = br.ReadInt16();
                    }
                }
                else if (channelCount == 2)
                {
                    for (int i = 0; i < sampleCount; i++)
                    {
                        L[i] = br.ReadInt16();
                        R[i] = br.ReadInt16();
                    }
                }

                return (sampleRate, L, R);
            }
        }
    }
}
