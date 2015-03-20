using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BMS
{
    public class GdiRender : CommonRender
    {
        Graphics canvas;
        public GdiRender(Graphics g)
        {
            canvas = g;
        }

        public Rectangle Rect { get; set; }

        public override void PlayImage(string imgNo)
        {
            var image = getImage(imgNo);
            if (image == null)
                return;

            canvas.DrawImage(image, Rect);
        }


        int width = 50;
        int height = 50;
        int P2offset = 100;
        int margin = 20;
        protected override void P1ActionImage(string action, string bgmNo, bool emptyBeat)
        {
            if (emptyBeat)
                return;

            var index = int.Parse(action);
            canvas.FillRectangle(Brushes.White, (index - 1) * (30 + 10) - 3, 36 * 10 - 3, 36, 16);

            /*

            var x = (index - 1) * (width + margin);


            canvas.FillRectangle(Brushes.Black, x, 0, width, height);
            if (emptyBeat)
                return;

            var font = new Font("宋体", 30.0f, FontStyle.Bold);
            canvas.DrawString(action, font, Brushes.White, x, 0);
            */
        }

        protected override void P2ActionImage(string action, string bgmNo, bool emptyBeat)
        {
            if (emptyBeat)
                return;

            var index = int.Parse(action);
            canvas.FillRectangle(Brushes.White, (index - 1) * (30 + 10) - 3, 36 * 10 - 3, 36, 16);

            /*
            var index = int.Parse(action);

            var x = (index - 1) * (width + margin);


            canvas.FillRectangle(Brushes.Black, x, height + P2offset, width, height);
            if (emptyBeat)
                return;

            var font = new Font("宋体", 30.0f, FontStyle.Bold);
            canvas.DrawString(action, font, Brushes.White, x, height + P2offset);
            */
        }
    }
}
