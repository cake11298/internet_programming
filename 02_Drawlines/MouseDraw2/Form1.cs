using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MouseDraw2
{ 
    public partial class Form1 : Form
    {
        private List<Stroke> strokes;     // 儲存所有線條
        private Stroke currentStroke;     // 當前正在繪製的線條
        private bool isDrawing;           // 標示是否正在繪製

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;   // 防止閃爍
            strokes = new List<Stroke>(); // 初始化線條集合
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MouseDown += new MouseEventHandler(MainForm_MouseDown);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(MainForm_MouseUp);
            this.Paint += new PaintEventHandler(MainForm_Paint);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 建立新的 Stroke
                currentStroke = new Stroke
                {
                    StrokeColor = Color.Blue, // 可以改成 UI 選擇的顏色
                    Thickness = 2,            // 可以改成 UI 選擇的粗細
                    Tool = ToolType.Pen       // 可以改成 UI 選擇的工具
                };
                currentStroke.Points.Add(e.Location);
                isDrawing = true;
            }
        }
        public enum ToolType
        {
            Pen,
            Eraser
        }
        public class Stroke
        {
            public List<Point> Points { get; set; } = new List<Point>();
            public Color StrokeColor { get; set; } = Color.Blue;
            public int Thickness { get; set; } = 2;
            public ToolType Tool { get; set; } = ToolType.Pen;
        }
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && currentStroke != null)
            {
                currentStroke.Points.Add(e.Location);
                this.Invalidate(); // 重新繪製
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && currentStroke != null)
            {
                isDrawing = false;
                strokes.Add(currentStroke); // 完成一條線，加入集合
                currentStroke = null;
                this.Invalidate();
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // 繪製所有 Stroke
            foreach (var stroke in strokes)
            {
                using (var pen = new Pen(
                    stroke.Tool == ToolType.Eraser ? Color.White : stroke.StrokeColor,
                    stroke.Thickness))
                {
                    for (int i = 1; i < stroke.Points.Count; i++)
                    {
                        g.DrawLine(pen, stroke.Points[i - 1], stroke.Points[i]);
                    }
                }
            }

            // 繪製正在畫的 Stroke（尚未 MouseUp）
            if (currentStroke != null && currentStroke.Points.Count > 1)
            {
                using (var pen = new Pen(
                    currentStroke.Tool == ToolType.Eraser ? Color.White : currentStroke.StrokeColor,
                    currentStroke.Thickness))
                {
                    for (int i = 1; i < currentStroke.Points.Count; i++)
                    {
                        g.DrawLine(pen, currentStroke.Points[i - 1], currentStroke.Points[i]);
                    }
                }
            }
        }
    }
}