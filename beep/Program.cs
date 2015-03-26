using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using BMS;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Utils;

namespace beep
{

    class testSampleProvider : ISampleProvider
    {
        public int DuplicatePerSample
        {
            get;
            set;
        }

        Resampler s;
        ISampleProvider _p;

        public testSampleProvider(ISampleProvider p)
        {
            s = new Resampler(true, 1.0, 100.0);
            _p = p;
        }

        private float[] sourceBuffer = new float[640000];

        public int Read(float[] buffer, int offset, int count)
        {
            var n = _p.Read(buffer, offset, count);
            var sb = new SampleBuffers(buffer, offset, n, sourceBuffer, 0, sourceBuffer.Length);

            s.process(DuplicatePerSample * 8.0, sb, n == 0);

            return sb.GetOutCount();            
        }

        public WaveFormat WaveFormat
        {
            get { return _p.WaveFormat; }
        }
    }

    class DuplicateSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private float[] sourceBuffer;

        public int DuplicatePerSample
        {
            get;
            set;
        }

        public DuplicateSampleProvider(ISampleProvider provider)
        {
            source = provider;
            DuplicatePerSample = 0;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var slice = DuplicatePerSample + 1;
            var c = count / slice;
            BufferHelpers.Ensure(sourceBuffer, c);
            var n = source.Read(sourceBuffer, 0, c);

            c = 0;
            for (int i = 0; i < n; )
            {
                for (int j = 0; j < slice; ++j)
                {
                    for (int k = 0; k < source.WaveFormat.Channels; ++k)
                    {
                        if (j == 0) 
                        {
                            buffer[offset++] = sourceBuffer[i + k];
                        }
                        else
                        {
                            buffer[offset++] = sourceBuffer[i + k] * j;
                        }
                        ++c;
                    }
                }

                i += source.WaveFormat.Channels;
            }


            return c;
        }

        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }
    }



    static class Program
    {

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Debug.AutoFlush = true;
            Debug.Listeners.Add(new TextWriterTraceListener("debug.log"));
            //var sc = SCFile.Open("sample.txt");
            //var a = sc.ToActions();
            //Console.ReadLine();

            //var reader2 = new AudioFileReader("この涙を君に捧ぐ Short.mp3");

            //var raw = new RawSourceWaveStream(reader1, reader1.WaveFormat);

            ////var media = new MediaFoundationReader("この涙を君に捧ぐ Short.mp3");
            ////var block = new BlockAlignReductionStream(media);
            ////var vol = new VolumeSampleProvider(reader.ToSampleProvider());
            ////var wo = new WasapiOut(AudioClientShareMode.Shared, 0);
            ////wo.Init(vol);
            //var wav1 = new WaveOut();
            //wav1.Init(reader1);
            //var cb = WaveCallbackInfo.FunctionCallback();

            ////var wav2 = new WaveOut() ;
            ////wav2.Init(reader2);
            //wav1.Play();
            ////Thread.Sleep(1000);
            ////wav2.Play();
            ////wo.Play();


            //int count = 0;
            //while (wav1.PlaybackState != PlaybackState.Stopped)
            //{
            //    count++;
            //    //if (count % 10 == 0)
            //    //    wav2.Stop();
            //    //if (count % 20 == 0)
            //    //    wav2.Play();
            //    //reader1.Volume = (float)Math.Sin(count / Math.PI);
            //    //vol.Volume = (float)Math.Sin(count++/Math.PI);
            //    Thread.Sleep(1000);
            //}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
