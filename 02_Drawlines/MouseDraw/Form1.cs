using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/*
 * 在 C# 中，DoubleBuffered 是一種技術，用於減少或消除界面重繪時可能出現的閃爍現象。
 * 這個技術通過在記憶體中創建一個隱藏的緩衝區（即一個位圖）來繪製所有內容，然後一次性
 * 將這個緩衝區的內容繪製到螢幕上，而不是逐行逐點地直接在螢幕上繪製。
 * 這樣可以使界面顯得更加平滑，減少不必要的重繪過程，從而消除閃爍。
 */
namespace MouseDraw
{
    public partial class Form1: Form
    {
        private List<Point> points;  // 儲存經過的所有點
        private bool isDrawing;      // 標示是否正在繪製
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;  // 防止閃爍，開啟雙緩衝
            points = new List<Point>();  // 初始化點的列表
        }
        // 載入表單時註冊滑鼠事件
        private void Form1_Load(object sender, EventArgs e)
        {
            this.MouseDown += new MouseEventHandler(MainForm_MouseDown);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(MainForm_MouseUp);
            this.Paint += new PaintEventHandler(MainForm_Paint);
        }
        // 鼠標按下事件，開始記錄起始點
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 若是想保留每一條線，則不要清空points
                points.Clear();  // 每次開始新畫時，清空過去的點
                points.Add(e.Location);  // 記錄起始點
                isDrawing = true;  // 開始繪製
            }
        }

        // 鼠標移動事件，記錄經過的點並連接
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                points.Add(e.Location);  // 記錄當前滑鼠位置
                this.Invalidate();  // 重新繪製表單
            }
        }

        // 鼠標放開事件，停止繪製
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;  // 結束繪製
                this.Invalidate();  // 重新繪製表單，顯示所有連接的點
            }
        }

        // Paint 事件，繪製經過的所有連線
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Blue, 2);  // 使用藍色畫筆，粗細為2

            if (points.Count > 1)
            {
                // 遍歷所有點，並連接它們
                for (int i = 1; i < points.Count; i++)
                {
                    g.DrawLine(pen, points[i - 1], points[i]);
                }
            }
        }
    }
}
