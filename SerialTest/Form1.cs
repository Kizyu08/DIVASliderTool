using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

using System.Runtime.InteropServices;
using System.Threading;


namespace SerialTest
{
    // Win32APIを呼び出すためのクラス
    public class win32api
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // 仮想キーコードをスキャンコードに変換
        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        public extern static int MapVirtualKey(
            int wCode, int wMapType);
    }

    public partial class Form1 : Form
    {
        string[] commands =
        {
            //コマンド一覧
            //[ヘッダ(0xff固定値)][コマンドコード][実データ長][実データ][sum]
            //
            "FF1000F1",//0接続確認
            "FFF00011",//1デバイス情報
            "FF0300FE",//2スキャン開始
            "FF02613F",//3LED点灯ヘッダ＋ [ ([blue] [red] [green]) x 32] [sum]
            //LEDダンプデータ？
            "ff02613f22fb2831e83e43d05756b87068a0897b89a18d71baa059d3b241eca454ee9469ee837fee7395ee62abee52c0ee41d6ee31ecee33efdc37f1ca3bf3b73ff5a443f79147f97f4bfb6c4ffdfc5964e45579c8528fac4fa5914cbb7549d05946e63d4368ff02613f22fa2b35e24447ca5d5ab2766c9b8e7f83a7916bc0a453d9b143eea059ee906eee7f84ee6f9aee5eb0ee4ec5ee3ddbee30eeea34f0d838f2c53cf4b240f69f44f88d48fa7a4cfc6753f95769dd547ec25194a64eaa8a4bc06e48d55345eb374269ff02613f27f43139dc4a4cc5635ead7c719594837dad9666c6a84edfad48ee9c5eee8c74ee7b89ee6b9fee5ab5ee4acbee39e0ee31eee635f0d339f2c13df4ae41f69b45f88849fa764dfc6358f2566ed75384bb50999f4daf834ac56847db4c44f0304165"
        };

        byte[] keys =
        {
            (byte)Keys.A,
            (byte)Keys.Z,
            (byte)Keys.S,
            (byte)Keys.X,
            (byte)Keys.D,
            (byte)Keys.C,
            (byte)Keys.F,
            (byte)Keys.V,
            (byte)Keys.G,
            (byte)Keys.B,
            (byte)Keys.H,
            (byte)Keys.N,
            (byte)Keys.J,
            (byte)Keys.M,
            (byte)Keys.K,
            (byte)188
        };
        string lastPdaSlider = "00000000000000000000000000000000";
        string lastUniSlider = "0000000000000000";
        
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                portComboBox.Items.Add(port);
            }
            foreach (string command in commands)
            {
                TXcomboBox.Items.Add(command);
            }
            if (portComboBox.Items.Count > 0)
                portComboBox.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort1.BaudRate = 115200;
            serialPort1.Parity = Parity.None;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Handshake = Handshake.None;
            serialPort1.PortName = portComboBox.Text;
            serialPort1.Open();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                byte[] buffer = hexStringToBytes(TXcomboBox.Text);
                serialPort1.Write(buffer, 0, buffer.Length);
            }
        }

        private void sendPacket(string strPacket)
        {
            byte[] packet = hexStringToBytes(strPacket);
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(packet, 0, packet.Length);
            }
        }

        delegate void SetTextCallback(string text);
        private void Response(string text)
        {
            if (textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(Response);
                BeginInvoke(d, new object[] { text });
            }
            else
            {
                textBox1.AppendText(text + "\r\n");
            }
        }
        
        private void sliderResponse(string text)
        {
            if (textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(sliderResponse);
                BeginInvoke(d, new object[] { text });
            }
            else
            {
                resultTextBox.Text = text + "\r\n";
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //ヘッダ読み取り
            byte[] header = new byte[3];
            header[0] = (byte)serialPort1.ReadByte();
            header[1] = (byte)serialPort1.ReadByte();
            header[2] = (byte)serialPort1.ReadByte();

            string strHeader = BitConverter.ToString(header).Replace("-", string.Empty);
            int len = header[2];
            Response("header:" + strHeader + " length:" + len);

            //データ部読み取り

            byte[] data = new byte[len];
            if (len != 0)
            {
                for (int i = 0; i < len; i++)
                {
                    data[i] = (byte)serialPort1.ReadByte();
                }
                string strData = BitConverter.ToString(data).Replace("-", string.Empty);
                Response("data:" + strData);
            }

            //sum読み取り
            
            int tmp = (byte)serialPort1.ReadByte();
            byte[] chkSum;
            if (tmp==100 && len==18)
            {
                chkSum = new byte[3];
                chkSum[0] = (byte)tmp;
                chkSum[1] = (byte)serialPort1.ReadByte();
                chkSum[2] = (byte)serialPort1.ReadByte();
            }else if (tmp == 253)
            {
                chkSum = new byte[2];
                chkSum[0] = (byte)tmp;
                chkSum[1] = (byte)serialPort1.ReadByte();
            }
            else
            {
                chkSum = new byte[1];
                chkSum[0] = (byte)tmp;
            }

            string strSum = BitConverter.ToString(chkSum).Replace("-", string.Empty);
            Response("SUM:" + strSum);
            //Response("mySUM:" + calcSUM(bytesToString(header) + bytesToString(data)).ToString("X2"));

            
            if (header[1] == 0x01)
            {
                string pdaslider = readSliderPacket(data);
                updateKeys(pdaslider);
                sendPacket(assembleTouchedSliderLED(pdaslider));
                //sliderResponse("pdaSlider:" + pdaslider);
                lastPdaSlider = pdaslider;
            }

        }

        private void updateKeys(string pdaslider)
        {
            string uniSlider = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 16; i++)
            {
                if (pdaslider[i*2] == '1' || pdaslider[(i*2)+1] == '1')
                {
                    uniSlider += '1';
                }
                else
                {
                    uniSlider += '0';
                }
                if (lastUniSlider[i] != uniSlider[i])
                {
                    if(uniSlider[i] == '1')//0->1
                    {
                        win32api.keybd_event(keys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(keys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            lastUniSlider = uniSlider;
        }

        private string readSliderPacket(byte[] datas)
        {
            //ビットシフト式
            //for (int j = 0; j < 16; j++)
            //{
            //    if (data[j * 2] != 0 | data[(j * 2) + 1] != 0)
            //    {
            //        uniSlider = (ushort)(uniSlider | (1 << j));
            //    }
            //    else
            //    {
            //        uniSlider = (ushort)(uniSlider & ~(1 << j)); ;
            //    }
            //}
            //return Convert.ToString(uniSlider, 2).PadLeft(16, '0');
            string pdaslider = "";
            int j;
            foreach(byte data in datas)
            {
                if (data != 0) { j = 1; } else { j = 0; }
                pdaslider += j;
            }
            return pdaslider;
        }

        private string assembleTouchedSliderLED(string pdaSlider)
        {
            string basecolor = "00fee6";
            string touchcolor = "fefefe";
            string result = commands[3];

            foreach (char c in pdaSlider)
            {
                if (c == '0')
                {
                    result += basecolor;
                }
                else
                {
                    result += touchcolor;
                }
            }
            result += calcSUM(result).ToString("X2");

            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
        }

        public static byte[] hexStringToBytes(string str)
        {
            int length = str.Length / 2;
            byte[] bytes = new byte[length];
            int j = 0;
            for (int i = 0; i < length; i++) {
                bytes[i] = Convert.ToByte(str.Substring(j, 2), 16);
                j += 2;
            }
            return bytes;
        }

        private static string bytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        private void LEDSendButton_Click(object sender, EventArgs e)
        {
            string packet = commands[3];
            for (int i = 0; i < 32;i++)
            {
                packet += LEDTextBox.Text;
            }
            packet += calcSUM(packet).ToString("X2");
            sendPacket(packet);
            Response("Send:" + packet);

        }

        private byte calcSUM(string packet)
        {
            int sum = 0;
            byte[] bytesPacket = hexStringToBytes(packet);
            if (bytesPacket.Length != 0)
            {
                for(int i=0; i < bytesPacket.Length; i++)
                {
                    sum += bytesPacket[i];
                }
                sum = 0x100 - (sum & 0xFF);
            }
            return (byte)sum;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sendPacket(commands[2]);
        }

        private void scanButtonClicked(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void scanStopButton_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }
}
