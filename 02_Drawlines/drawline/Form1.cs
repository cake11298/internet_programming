using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace drawline
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // 在 Paint 事件中繪製直線
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // 使用 Graphics 物件來繪製
            Graphics g = e.Graphics;

            // 定義直線的起點和終點
            Point startPoint = new Point(50, 50);
            Point endPoint = new Point(200, 200);

            // 創建一個畫筆（可以設定顏色與粗細）
            Pen pen = new Pen(Color.Blue, 2);

            // 使用 DrawLine 方法繪製直線
            g.DrawLine(pen, startPoint, endPoint);
        }
        // 載入表單時，註冊 Paint 事件
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Paint += new PaintEventHandler(MainForm_Paint);
        }       

    }
}
