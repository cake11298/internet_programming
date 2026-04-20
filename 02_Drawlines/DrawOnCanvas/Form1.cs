using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawOnCanvas
{
    public partial class Form1: Form
    {
        private List<List<Point>> allLines;   // 儲存所有繪製的線條，每條線由一系列的點組成
        private List<Point> currentLine;      // 當前正在繪製的線
        private bool isDrawing;               // 標示是否正在繪製
        private Panel canvasPanel;            // 畫布（Panel）
        public Form1()
        {
            InitializeComponent();
            allLines = new List<List<Point>>();  // 初始化所有線條的列表
            currentLine = new List<Point>();      // 初始化當前線條的點列表

            // 初始化畫布 Panel
            canvasPanel = new Panel
            {
                Location = new Point(50, 60),  // 設定畫布的起始位置
                Size = new Size(600, 400),     // 設定畫布大小
                BorderStyle = BorderStyle.FixedSingle,  // 設定邊框
            };
            this.Controls.Add(canvasPanel);  // 將 Panel 加入到表單中

            // 註冊 Panel 的滑鼠事件
            canvasPanel.MouseDown += CanvasPanel_MouseDown;
            canvasPanel.MouseMove += CanvasPanel_MouseMove;
            canvasPanel.MouseUp += CanvasPanel_MouseUp;
            canvasPanel.Paint += CanvasPanel_Paint;  // 重新繪製時觸發
        }

        // 載入表單時註冊滑鼠事件
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Canvas Drawing Example";  // 設定窗體標題
            this.Size = new Size(500, 500);  // 設定窗體大小
        }
        // 鼠標按下事件，開始記錄起始點
        private void CanvasPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                currentLine.Clear();  // 開始新的一條線時清空當前線條
                currentLine.Add(e.Location);  // 記錄起始點
                isDrawing = true;  // 開始繪製
            }
        }

        // 鼠標移動事件，記錄經過的點並連接
        private void CanvasPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                // 只記錄在畫布區域內的點
                if (canvasPanel.ClientRectangle.Contains(e.Location))
                {
                    currentLine.Add(e.Location);  // 記錄當前滑鼠位置
                    canvasPanel.Invalidate();  // 重新繪製畫布
                }
            }
        }

        // 鼠標放開事件，停止繪製並將線條添加到所有線條列表
        private void CanvasPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;  // 結束繪製
                if (currentLine.Count > 1)  // 確保至少有兩個點才能構成一條線
                {
                    allLines.Add(new List<Point>(currentLine));  // 保存當前線條
                }
                canvasPanel.Invalidate();  // 重新繪製畫布，顯示所有線條
            }
        }

        // Paint 事件，繪製所有已經繪製的線條
        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Blue, 2);  // 使用藍色畫筆，粗細為2

            // 繪製所有已經畫過的線條
            foreach (var line in allLines)
            {
                for (int i = 1; i < line.Count; i++)
                {
                    g.DrawLine(pen, line[i - 1], line[i]);
                }
            }

            // 繪製當前正在繪製的線條
            if (currentLine.Count > 1)
            {
                for (int i = 1; i < currentLine.Count; i++)
                {
                    g.DrawLine(pen, currentLine[i - 1], currentLine[i]);
                }
            }
        }
    }
}
