using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace chessBoard
{
    public partial class Form1 : Form
    {
        private int cellSize = 20;
        private int numCells = 18;
        private List<Point> stones = new List<Point>(); // 存放已下棋子座標

        public Form1()
        {
            InitializeComponent();

            panelBoard.BackColor = Color.White;
            panelBoard.Width = cellSize * numCells+1;
            panelBoard.Height = cellSize * numCells+1;
            panelBoard.Paint += PanelBoard_Paint;
            panelBoard.MouseClick += PanelBoard_MouseClick;
        }

        private void PanelBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen gridPen = Pens.Gray;

            // 畫水平線
            for (int i = 0; i <= numCells; i++)
            {
                int y = i * cellSize;
                g.DrawLine(gridPen, 0, y, numCells * cellSize, y);
            }

            // 畫垂直線
            for (int i = 0; i <= numCells; i++)
            {
                int x = i * cellSize;
                g.DrawLine(gridPen, x, 0, x, numCells * cellSize);
            }

            // 畫棋子（黑圓形）
            foreach (var pt in stones)
            {
                int radius = 6;
                int cx = pt.X * cellSize;
                int cy = pt.Y * cellSize;

                g.FillEllipse(Brushes.Black, cx - radius, cy - radius, radius * 2, radius * 2);
            }
        }

        private void PanelBoard_MouseClick(object sender, MouseEventArgs e)
        {
            // 找出最接近的交點
            int x = (int)Math.Round((double)e.X / cellSize);
            int y = (int)Math.Round((double)e.Y / cellSize);

            Point newStone = new Point(x, y);

            if (!stones.Contains(newStone))
            {
                stones.Add(newStone);
                panelBoard.Invalidate(); // 重新繪製
            }
        }
    }
}
