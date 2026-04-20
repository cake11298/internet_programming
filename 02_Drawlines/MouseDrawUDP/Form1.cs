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
using System.Threading;   //匯入多執行緒功能函數

namespace MouseDrawUDP
{
    public partial class Form1 : Form
    {
        UdpClient U; //宣告UDP通訊物件
        Thread Th;   //宣告監聽用執行緒        
        private List<Point> points;  // 儲存經過的所有點
        private bool isDrawing;      // 標示是否正在繪製
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;  // 防止閃爍，開啟雙緩衝
            points = new List<Point>();  // 初始化點的列表
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += " " + MyIP();   //顯示本機IP於標題列
            this.MouseDown += new MouseEventHandler(MainForm_MouseDown);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(MainForm_MouseUp);
            this.Paint += new PaintEventHandler(MainForm_Paint);
        }
        //找出本機IP
        private string MyIP()
        {
            string hn = Dns.GetHostName();                          //取得本機電腦名稱
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList;      //取得本機IP陣列(可能有多個)
            foreach (IPAddress it in ip)                            //列舉各個IP
            {
                if (it.AddressFamily == AddressFamily.InterNetwork) //如果是IPv4格式
                {
                    return it.ToString();                           //傳回此IP字串
                }
                /*
                 * AddressFamily成員會指定 將用來解析地址的尋址配置Socket。 
                 * 例如，InterNetwork 表示當連線到端點時 Socket ，應該要有IP第4版位址。
                 */
            }
            return "";                                              //找不到合格IP，回傳空字串
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;//忽略跨執行緒錯誤
            Th = new Thread(Listen); //建立監聽執行緒，目標副程序→Listen
            Th.Start(); //啟動監聽執行緒
            button1.Enabled = false; //即使按鍵失效，不能(也不需要)重複開啟監聽
        }
        //監聽副程序
        private void Listen()
        {
            int Port = int.Parse(textBox3.Text); //設定監聽用的通訊埠
            U = new UdpClient(Port);             //監聽UDP監聽器實體
            //建立本機端點資訊
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);
            while (true) //持續監聽的無限迴圈→有訊息(True)就處理，無訊息就等待！
            {
                byte[] B = U.Receive(ref EP);                //訊息到達時讀取資訊到B陣列
                string data = Encoding.Default.GetString(B); //翻譯B陣列為字串
                points = StringToList(data);
                this.Invalidate();  // 標記控制項需要重繪，系統稍後會觸發 Paint
            }
        }

        //關閉監聽執行續(如果有的話)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Th?.Abort(); //關閉監聽執行續
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
                this.Invalidate();  // 標記控制項需要重繪，系統稍後會觸發 Paint
            }
        }

        // 鼠標放開事件，停止繪製
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = false;  // 結束繪製
                string IP = textBox1.Text;                   //設定發送目標IP
                int Port = int.Parse(textBox2.Text);         //設定發送目標Port                                                              
                string data = ListToString(points);          // 將 List<Point> 轉換為 string
                byte[] B = Encoding.Default.GetBytes(data);  //字串翻譯成位元組陣列
                UdpClient S = new UdpClient();               //建立UDP通訊器
                S.Send(B, B.Length, IP, Port);               //發送資料到指定位置
                S.Close();                                   //關閉通訊器
                this.Invalidate();                           // 重新繪製表單，顯示所有連接的點
            }
        }

        // Paint 事件，繪製經過的所有連線
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics; //在 Paint 事件中透過 PaintEventArgs e.Graphics 取得Graphics
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
        static List<Point> StringToList(string input)
        {
            // 用分號分隔字串，得到每個 "X,Y" 的部分
            var pointStrings = input.Split(';');
            var points = new List<Point>();

            foreach (var pointString in pointStrings)
            {
                // 進一步解析 "X,Y" 格式，並將 X 和 Y 轉換為整數
                var coordinates = pointString.Split(',');
                if (coordinates.Length == 2)
                {
                    int x = int.Parse(coordinates[0]);
                    int y = int.Parse(coordinates[1]);

                    // 創建 Point 並加入到 List<Point>
                    points.Add(new Point(x, y));
                }
            }

            return points;
        }
        static string ListToString(List<Point> points)
        {
            // 使用 StringBuilder 來組合字串
            var pointStrings = new List<string>();

            foreach (var point in points)
            {
                // 將每個 Point 轉換為 "X,Y" 格式的字串
                pointStrings.Add($"{point.X},{point.Y}");
            }

            // 使用逗號將所有 "X,Y" 格式的字串連接起來
            return string.Join(";", pointStrings); // 可以選擇不同的分隔符，例如 "，"、";" 等
        }
    }
}
