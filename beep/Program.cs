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

namespace beep
{
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

            //var reader1 = new AudioFileReader("この涙を君に捧ぐ Short.mp3");
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
