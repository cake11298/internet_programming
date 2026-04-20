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
using System.Threading;   //匯入多執行續功能函數
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TAP
{
    public partial class Form1 : Form
    {
        UdpClient U; //宣告UDP通訊物件
        bool listening = false; // 控制迴圈用旗標

        public Form1()
        {
            InitializeComponent();
        }
        //表單載入
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += " " + MyIP();                              //顯示本機IP於標題列
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
        // 改用 ReceiveAsync 搭配 async/await
        private async void button1_Click(object sender, EventArgs e)
        {
            int Port = int.Parse(textBox1.Text);
            U = new UdpClient(Port);
            listening = true;            
            button1.Enabled = false;

            await ListenAsync(); // 啟動非同步監聽
        }
        private async Task ListenAsync()
        {
            int Port = int.Parse(textBox1.Text); //設定監聽用的通訊埠
            U = new UdpClient(Port);             //監聽UDP監聽器實體

            while (listening)
            {
                try
                {
                    UdpReceiveResult result = await U.ReceiveAsync(); // 非同步接收
                    string msg = Encoding.Default.GetString(result.Buffer);

                    // 更新 UI 要用 Invoke
                    this.Invoke((Action)(() =>
                    {
                        textBox2.Text = msg;
                    }));
                }
                catch (ObjectDisposedException)
                {
                    // Socket 已關閉，跳出迴圈
                    break;
                }
                catch (Exception ex)
                {
                    // 其他錯誤處理
                    MessageBox.Show("接收錯誤: " + ex.Message);
                    break;
                }
            }
        }
        // 發送 UDP訊息 也改成非同步
        private async void button2_Click(object sender, EventArgs e)
        {
            string IP = textBox3.Text;
            int Port = int.Parse(textBox4.Text);
            byte[] B = Encoding.Default.GetBytes(textBox5.Text);

            using (UdpClient S = new UdpClient())
            {
                await S.SendAsync(B, B.Length, IP, Port); // 非同步發送
            }
        }
        // 因為 ReceiveAsync 是非同步迴圈，關閉時只要呼叫 U.Close()，迴圈會丟出 ObjectDisposedException，就能跳出迴圈，不需要 Thread.Abort()。
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            listening = false;
            U?.Close(); // 安全關閉，避免 NullReferenceException
        }
        /*
        //啟動監聽按鈕程序
        private void button1_Click(object sender, EventArgs e)
        {
            //Control.CheckForIllegalCrossThreadCalls = false;//忽略跨執行續錯誤
            Th = new Thread(Listen); //建立監聽執行續，目標副程序→Listen
            Th.Start(); //啟動監聽執行續
            button1.Enabled = false; //使按鍵失效，不能(也不需要)重複開啟監聽
        }

        private void Listen()
        {
            int Port = int.Parse(textBox1.Text); //設定監聽用的通訊埠
            U = new UdpClient(Port);             //監聽UDP監聽器實體
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port);

            while (true) //持續監聽的無限迴圈
            {
                byte[] B = U.Receive(ref EP); //訊息到達時讀取資訊到B陣列

                string msg = Encoding.Default.GetString(B);
                // 使用 Invoke 更新 UI
                this.Invoke((Action)(() =>
                {
                    textBox2.Text = msg;
                }));
            }
        }
        
        //發送UDP訊息
        private void button2_Click(object sender, EventArgs e)
        {
            string IP = textBox3.Text;                           //設定發送目標IP
            int Port = int.Parse(textBox4.Text);                 //設定發送目標Port
            byte[] B = Encoding.Default.GetBytes(textBox5.Text); //字串翻譯成位元組陣列
            UdpClient S = new UdpClient();                       //建立UDP通訊器
            S.Send(B, B.Length, IP, Port);                       //發送資料到指定位置
            S.Close();                                           //關閉通訊器
        }        
        //關閉監聽執行續(如果有的話)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Th.Abort(); //關閉監聽執行續
                U.Close();  //關閉監聽器
            }
            catch
            {
                //忽略錯誤，程式繼續執行
            }
        }
        */
    }
}
