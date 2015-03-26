using BMS;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace beep
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ActionPlayer ap;
        GdiRender render;
        Stopwatch watcher;
        Graphics canvas;

        enum STATE { None, Init, Ready, Running, Stop}

        STATE _state;
        STATE state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;

                switch (_state)
                {
                    case STATE.None:
                        button1.Enabled = button2.Enabled = button3.Enabled = false;
                        toolStripStatusLabel1.Text = "Loading...";
                        return;

                    case STATE.Init:
                        button1.Enabled = true;
                        button2.Enabled = button3.Enabled = false;
                        break;

                    case STATE.Ready:
                    case STATE.Stop:
                        button1.Enabled = button3.Enabled = true;
                        button2.Enabled = false;
                        break;

                    case STATE.Running:
                        button2.Enabled = true;
                        button1.Enabled = button3.Enabled = false;
                        break;
                }

                toolStripStatusLabel1.Text = _state.ToString();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (state != STATE.Init)
            {
                Stop();
            }

            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            state = STATE.None;

            backgroundWorker1.RunWorkerAsync();
        }

        private void Start()
        {
            watcher = Stopwatch.StartNew();
            ap.Start((float)watcher.Elapsed.TotalSeconds);
            timer1.Start();

            state = STATE.Running;
        }

        private void Stop()
        {
            if (state == STATE.Running)
            {
                timer1.Stop();
                watcher.Stop();
            }

            render.Dispose();

            state = STATE.Stop;
        }


        Graphics gameCanvas;
        ActionGameLayout gameLayout;

        private void timer1_Tick(object sender, EventArgs e)
        {
            gameCanvas.Clear(Color.Black);
            for (int i = 0; i < 8; ++i)
            {
                gameCanvas.DrawRectangle(Pens.Yellow, i * (30 + 10), 36 * 10, 30, 10);
            }

            var running = ap.Play((float)watcher.Elapsed.TotalSeconds);
            var moreData = gameLayout.Process();
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 40; ++j)
                {
                    if (gameLayout.Layout[i, j] == 0)
                        continue;

                    if (j == 0)
                    {
                        if (gameLayout.FirstHeight != 0)
                            gameCanvas.FillRectangle(Brushes.White, i * (30 + 10), 0, 30, gameLayout.FirstHeight);
                    }
                    else
                    {
                        gameCanvas.FillRectangle(Brushes.White, i * (30 + 10), (j - 1) * 10 + gameLayout.FirstHeight, 30, 10);
                    }
                }
            }

            if (!render.AutoPlay)
            {
                List<int> actions = new List<int>();
                foreach (var key in keys)
                {
                    try
                    {
                        var col = keyMap[key];
                        render.P1Hit(col);
                        var a = Convert.ToInt32(col);
                        actions.Add(a);
                        gameCanvas.FillRectangle(Brushes.White, (a - 1) * (30 + 10) - 3, 36 * 10 - 3, 36, 16);
                    }
                    catch { }
                }
                gameLayout.Hit(actions.ToArray());
            }
            

            pictureBox1.Invalidate();
            pictureBox2.Invalidate();

            if (!moreData)
            {
                Stop();
            }
        }

        static Dictionary<Keys, string> keyMap = new Dictionary<Keys, string>
        {
            {Keys.D1, "1"},
            {Keys.D2, "2"},
            {Keys.D3, "3"},
            {Keys.D4, "4"},
            {Keys.D5, "5"},
            {Keys.D6, "6"},
            {Keys.D7, "7"},
            {Keys.D8, "8"},
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            state = STATE.Init;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            canvas = Graphics.FromImage(pictureBox1.Image);

            pictureBox2.Image = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            gameCanvas = Graphics.FromImage(pictureBox2.Image);

            render = new GdiRender(gameCanvas);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = SCFile.Open(openFileDialog1.FileName);
            var actions = file.ToActions();
            ap = new ActionPlayer(actions, render);
            gameLayout = new ActionGameLayout(ap, 8, 40, 36, 30, 10);
            render.GameLayout = gameLayout;
            render.AutoPlay = true;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            state = STATE.Ready;
        }

        HashSet<Keys> keys = new HashSet<Keys>();

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            keys.Add(e.KeyCode);
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keys.Remove(e.KeyCode);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var reader1 = new AudioFileReader("++++#2.wav");
            //var sample = new DuplicateSampleProvider(reader1.ToSampleProvider());
            var sample = new testSampleProvider(reader1);
            sample.DuplicatePerSample = 8;
            //var wav = new WaveOut();
            //wav.Init(sample);
            //wav.Play();

            var wp = new SampleToWaveProvider(sample);
            byte[] buffer = new byte[wp.WaveFormat.AverageBytesPerSecond * 4 * sample.DuplicatePerSample];
            MemoryStream ms = new MemoryStream();
            do
            {
                var c = wp.Read(buffer, 0, buffer.Length);
                if (c == 0)
                    break;
                ms.Write(buffer, 0, c);
            } while (true);
            ms.Flush();

            waveViewer1.WaveStream = new RawSourceWaveStream(ms, wp.WaveFormat);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            var reader1 = new AudioFileReader("++++#2.wav");
            //var wav = new WaveOut();
            //wav.Init(reader1);
            //wav.Play();
            waveViewer1.WaveStream = reader1;
        }
    }
}
