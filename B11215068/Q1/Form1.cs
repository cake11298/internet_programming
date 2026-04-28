using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;         //匯入網路通訊協定相關參數
using System.Net.Sockets; //匯入網路插座功能函數
using System.Threading;
using System.Net.NetworkInformation;   //匯入多執行緒功能函數

namespace Q1
{
    public partial class Form1 : Form
    {
        UdpClient U; //宣告UDP通訊物件
        //自行決定同步或非同步的方式
        /*
        private List<List<Point>> allLines;   // 儲存所有繪製的線條，每條線由一系列的點組成
        private List<Point> currentLine;      // 當前正在繪製的線
        */

        private bool isDrawing;               // 標示是否正在繪製
        Thread Th;
        private List<Point> points;
        private Color currentColor = Color.Blue;
        private int currentThickness = 2;
        private List<Stroke> strokes = new List<Stroke>();
        private Stroke currentStroke;
        public enum ToolType { Pen }
        public class Stroke
        {
            public List<Point> Points { get; set; } = new List<Point>();
            public Color StrokeColor { get; set; } = Color.Blue;
            public int Thickness { get; set; } = 2;
        }

       

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;          // 防止閃爍，開啟雙緩衝
            points = new List<Point>();
            //allLines = new List<List<Point>>();  // 初始化所有線條的列表
            //currentLine = new List<Point>();     // 初始化當前線條的點列表
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //顯示本機IP於標題列
            this.Text = "Q1 資工系_B11215068_楊智宇" + MyIP();
            this.MouseDown += new MouseEventHandler(MainForm_MouseDown);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(MainForm_MouseUp);
            this.Paint += new PaintEventHandler(MainForm_Paint);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            //自行決定同步或非同步的方式建立監聽
            //建立經聽之後，讓按鍵失效，不能(也不需要)重複開啟監聽
            Th = new Thread(Listen);
            Th.IsBackground = true;
            Th.Start();
            button1.Enabled = false;
        }
        //監聽副程序(自行定義)
        private void Listen()
        {
            int Port = int.Parse(textBox3.Text);
            U = new UdpClient(Port);
            IPEndPoint EP = new IPEndPoint(IPAddress.Any, Port);
            while (true)
            {
                byte[] B = U.Receive(ref EP);
                string data = Encoding.Default.GetString(B);
                Stroke s = StringToStroke(data);
                if (s != null)
                {
                    this.Invoke((Action)(() =>
                    {
                        strokes.Add(s);
                        this.Invalidate();
                    }));
                }
            }
        }

        //關閉監聽執行續(如果有的話)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                //Th.Abort(); //關閉監聽執行緒
                U?.Close();  //關閉監聽器
            }
            catch
            {
                //忽略錯誤，程式繼續執行
            }
        }

        // 鼠標按下事件，開始記錄起始點
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //currentLine.Clear();  // 開始新的一條線時清空當前線條
                //currentLine.Add(e.Location);  // 記錄起始點
                currentStroke = new Stroke { StrokeColor = currentColor, Thickness = currentThickness };
                currentStroke.Points.Add(e.Location);
                isDrawing = true;  // 開始繪製
            }
        }

        // 鼠標移動事件，記錄經過的點並連接
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                currentStroke.Points.Add(e.Location);   
                this.Invalidate();  // 重新繪製表單
            }
        }

        // 鼠標放開事件，停止繪製
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && currentStroke != null)
            {
                isDrawing = false;
                strokes.Add(currentStroke);
                string IP = textBox1.Text;
                int Port = int.Parse(textBox2.Text);
                string data = StrokeToString(currentStroke);
                byte[] B = Encoding.Default.GetBytes(data);
                UdpClient S = new UdpClient();
                S.Send(B, B.Length, IP, Port);
                S.Close();
                this.Invalidate();
            }
        }

        // Paint 事件，繪製經過的所有連線
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (var stroke in strokes)
            {
                using (var pen = new Pen(stroke.StrokeColor, stroke.Thickness))
                {
                    for (int i = 1; i < stroke.Points.Count; i++)
                        g.DrawLine(pen, stroke.Points[i - 1], stroke.Points[i]);
                }
            }
            if (currentStroke != null && currentStroke.Points.Count > 1)
            {
                using (var pen = new Pen(currentStroke.StrokeColor, currentStroke.Thickness))
                {
                    for (int i = 1; i < currentStroke.Points.Count; i++)
                        g.DrawLine(pen, currentStroke.Points[i - 1], currentStroke.Points[i]);
                }
            }
        }
        // 自行決定是否採用序列與反序列化的函式
        public static List<List<Point>> StringToList(string str)
        {
            var pointsList = new List<List<Point>>();
            var listStrings = str.Split('|');

            foreach (var listStr in listStrings)
            {
                var innerList = new List<Point>();
                var pointStrings = listStr.Split(';');

                foreach (var pointStr in pointStrings)
                {
                    if (!string.IsNullOrEmpty(pointStr))
                    {
                        var parts = pointStr.Split(',');
                        var x = int.Parse(parts[0]);
                        var y = int.Parse(parts[1]);
                        innerList.Add(new Point(x, y));
                    }
                }

                pointsList.Add(innerList);
            }

            return pointsList;
        }


        static string StrokeToString(Stroke s)
        {
            string colorPart = $"COLOR:{s.StrokeColor.R},{s.StrokeColor.G},{s.StrokeColor.B}";
            string thickPart = $"THICK:{s.Thickness}";
            var pts = new List<string>();
            foreach (var p in s.Points)
                pts.Add($"{p.X},{p.Y}");
            string pointPart = "POINTS:" + string.Join(";", pts);
            return colorPart + "|" + thickPart + "|" + pointPart;
        }
        static Stroke StringToStroke(string data)
        {
            try
            {
                var parts = data.Split('|');
                var cRaw = parts[0].Replace("COLOR:", "").Split(',');
                var tRaw = parts[1].Replace("THICK:", "");
                var pRaw = parts[2].Replace("POINTS:", "").Split(';');
                Color c = Color.FromArgb(int.Parse(cRaw[0]), int.Parse(cRaw[1]), int.Parse(cRaw[2]));
                int thick = int.Parse(tRaw);
                var stroke = new Stroke { StrokeColor = c, Thickness = thick };
                foreach (var seg in pRaw)
                {
                    var xy = seg.Split(',');
                    if (xy.Length == 2)
                        stroke.Points.Add(new Point(int.Parse(xy[0]), int.Parse(xy[1])));
                }
                return stroke;
            }
            catch { return null; }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) currentColor = Color.Red;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked) currentColor = Color.Green;
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked) currentColor = Color.Blue;
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) currentColor = Color.Black;
        }
        private void numericUpDown1_CheckedChanged(object sender, EventArgs e)
        {
            currentThickness = (int)numericUpDown1.Value;
        }
        public string MyIP() { return " 140.118.126.38"; }
    }
}
