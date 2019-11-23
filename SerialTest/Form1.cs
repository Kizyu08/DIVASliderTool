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


namespace DIVASliderTool
{
    public partial class Form1 : Form
    {
        bool debug = true;
        bool sending = false;

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

        int mode = 0;
        string[] modes =
        {
            "ProjectDIVA",
            "seaurchin",
            "Nostalgia",
            "Musinx 4Key",
            "Musinx 6Key",
            "SEGAToolsChunithm"
        };

        //slider slider = new Chunithm();
        slider[] slider =
        {
            new Chunithm(),
            new Chunithm(),
            new Nostalgia(),
            new MUSYNX4(),
            new MUSYNX6(),
            new SEGAToolsChuni()
        };

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
            foreach (string mode in modes)
            {
                ModeComboBox.Items.Add(mode);
            }
            ModeComboBox.SelectedIndex = 0;
            if (portComboBox.Items.Count > 0)
                portComboBox.SelectedIndex = 0;
            logCheckBox1.Checked = debug;
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
            sending = true;
            strPacket += calcSUM(strPacket).ToString("X2");
            byte[] packet = hexStringToBytes(strPacket);
            if (serialPort1.IsOpen)
            {
                serialPort1.Write(packet, 0, packet.Length);
            }
            sending = false;
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
            if (header[0] != 0xff)
            {
                while (header[0] != 0xff) header[0] = (byte)serialPort1.ReadByte();
            }
            header[1] = (byte)serialPort1.ReadByte();
            header[2] = (byte)serialPort1.ReadByte();

            string strHeader = BitConverter.ToString(header).Replace("-", string.Empty);
            int len = header[2];
             if (debug) Response("header:" + strHeader + " length:" + len);

            //データ部読み取り

            byte[] data = new byte[len];
            if (len != 0)
            {
                for (int i = 0; i < len; i++)
                {
                    data[i] = (byte)serialPort1.ReadByte();
                    if (data[i] == 0xff) return;
                }
                string strData = BitConverter.ToString(data).Replace("-", string.Empty);
                if (debug) Response("data:" + strData);
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
            if (debug) Response("SUM:" + strSum);
            //Response("mySUM:" + calcSUM(bytesToString(header) + bytesToString(data)).ToString("X2"));

            //スキャンデータが来たらキー入力に変換&LEDに反映
            if (header[1] == 0x01)
            {
                string pdaslider = readSliderPacket(data);
                slider[mode].UpdateKeys(pdaslider);
                sendPacket(commands[3] + slider[mode].assembleTouchedSliderLED(pdaslider));
                sliderResponse(slider[mode].GetKeyState());
            }

        }

        

        private string readSliderPacket(byte[] datas)
        {
            string pdaslider = "";
            int j;
            foreach(byte data in datas)
            {
                if (data != 0) { j = 1; } else { j = 0; }
                pdaslider += j;
            }
            return pdaslider;
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            if (!sending){
                serialPort1.Close();
            }
        }

        //16進数文字列をbyte[]に
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

        //byte[]を16進数文字列に
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
            if(debug)Response("Send:" + packet);

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
            int i = portComboBox.SelectedIndex;
        }

        private void logCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            debug = logCheckBox1.Checked;
        }

        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mode = ModeComboBox.SelectedIndex;
        }
    }


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

    /// <summary>
    /// スライダー周り
    /// </summary>
    abstract class slider
    {
        public abstract string Basecolor { get; set; }
        public abstract string Touchcolor { get; set; }
        public abstract byte[] GameKeys { get; set; }
        public abstract void UpdateKeys(string pdaslider);
        public abstract string GetKeyState();
        /// <summary>
        /// スライダーのLED制御用パケット生成
        /// 単純にタッチした個所の色が変わるだけ
        /// </summary>
        /// <param name="pdaSlider">32のセンサの値を0or1で示したstring</param>
        /// <returns></returns>
        public virtual string assembleTouchedSliderLED(string pdaSlider)
        {
            string result = "";

            foreach (char c in pdaSlider)
            {
                if (c == '0')
                {
                    result += Basecolor;
                }
                else
                {
                    result += Touchcolor;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// チュウニズム
    /// </summary>
    class Chunithm:slider
    {
        public override string Basecolor { get; set; } = "00fee6";
        public override string Touchcolor { get; set; }  = "fefefe";
        private string UniSlider = "0000000000000000";
        private string LastUniSlider = "0000000000000000";

        public override string GetKeyState()
        {
            return UniSlider;
        }

        public override byte[] GameKeys { get; set; } =
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

        public override void UpdateKeys(string pdaslider)
        {
            UniSlider = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 16; i++)
            {
                if (pdaslider[i * 2] == '1' || pdaslider[(i * 2) + 1] == '1')
                {
                    UniSlider += '1';
                }
                else
                {
                    UniSlider += '0';
                }
                if (LastUniSlider[i] != UniSlider[i])
                {
                    if (UniSlider[i] == '1')//0->1
                    {
                        win32api.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastUniSlider = UniSlider;
        }
    }

    /// <summary>
    /// Nostalgia
    /// </summary>
    class Nostalgia:slider
    {
        public override string Basecolor { get; set; } = "fefefe";
        public override string Touchcolor { get; set; } = "000000";

        private string NostalKeys = "00000000000000000000000000000000";
        private string LastNostalKeys = "00000000000000000000000000000000";

        public override string GetKeyState()
        {
            return NostalKeys;
        }

        public override byte[] GameKeys { get; set; } =
        {
            (byte)Keys.D1,//1
            (byte)Keys.D2,
            (byte)Keys.D3,
            (byte)Keys.D4,
            (byte)Keys.D5,
            (byte)Keys.D6,
            (byte)Keys.D7,
            (byte)Keys.D8,
            (byte)Keys.D9,
            (byte)Keys.D0,
            (byte)Keys.Q,
            (byte)Keys.W,
            (byte)Keys.E,
            (byte)Keys.R,
            (byte)Keys.T,
            (byte)Keys.Y,
            (byte)Keys.U,
            (byte)Keys.I,
            (byte)Keys.O,
            (byte)Keys.P,
            (byte)Keys.A,
            (byte)Keys.S,
            (byte)Keys.D,
            (byte)Keys.F,
            (byte)Keys.G,
            (byte)Keys.H,
            (byte)Keys.J,
            (byte)Keys.K//,//28
            //(byte)Keys.L,
            //(byte)Keys.Z,
            //(byte)Keys.X,
            //(byte)Keys.C,
            //(byte)Keys.V,
            //(byte)Keys.B,
            //(byte)Keys.N,
            //(byte)Keys.M
        };

        public override string assembleTouchedSliderLED(string pdaSlider)
        {
            string result = "";

            for (int i = 0;  i<pdaSlider.Length; i++)
            {
                if(i<3 || i > 28)
                {
                    result += "000000";
                }
                else
                {
                    if (pdaSlider[i] == '0')
                    {
                        result += Basecolor;
                    }
                    else
                    {
                        result += Touchcolor;
                    }
                }
            }
            return result;
        }

        public override void UpdateKeys(string pdaslider)
        {
            NostalKeys = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 32; i++)
            {
                if (pdaslider[i] == '1')
                {
                    NostalKeys += '1';
                }
                else
                {
                    NostalKeys += '0';
                }
                if (LastNostalKeys[i] != NostalKeys[i] && 1<i && i < 29)
                {
                    if (NostalKeys[i] == '1')//0->1
                    {
                        win32api.keybd_event(GameKeys[i-2], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(GameKeys[i-2], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastNostalKeys = NostalKeys;
        }
    }

    /// <summary>
    /// MUSYNX 6Key
    /// </summary>
    class MUSYNX6 : slider
    {
        public override string Basecolor { get; set; } = "fefefe";
        public override string Touchcolor { get; set; } = "000000";
        private string[] keyColors =
        {
            "93e98e",
            "9af0d0",
            "96dbee",
            "be95fc",
            "e886f4",
            "ee9ac2"
        };

        private string MUS6Keys = "000000";
        private string LastMUS6Keys = "000000";

        public override string GetKeyState()
        {
            return MUS6Keys;
        }

        public override byte[] GameKeys { get; set; } =
        {
            //(byte)Keys.D1,//1
            //(byte)Keys.D2,
            //(byte)Keys.D3,
            //(byte)Keys.D4,
            //(byte)Keys.D5,
            //(byte)Keys.D6,
            //(byte)Keys.D7,
            //(byte)Keys.D8,
            //(byte)Keys.D9,
            //(byte)Keys.D0,
            //(byte)Keys.Q,
            //(byte)Keys.W,
            //(byte)Keys.E,
            //(byte)Keys.R,
            //(byte)Keys.T,
            //(byte)Keys.Y,
            //(byte)Keys.U,
            //(byte)Keys.I,
            //(byte)Keys.O,
            //(byte)Keys.P,
            //(byte)Keys.A,
            (byte)Keys.S,
            (byte)Keys.D,
            (byte)Keys.F,
            //(byte)Keys.G,
            //(byte)Keys.H,
            (byte)Keys.J,
            (byte)Keys.K,
            (byte)Keys.L//,
            //(byte)Keys.Z,
            //(byte)Keys.X,
            //(byte)Keys.C,
            //(byte)Keys.V,
            //(byte)Keys.B,
            //(byte)Keys.N,
            //(byte)Keys.M
        };

        public override string assembleTouchedSliderLED(string pdaSlider)
        {
            string result = "";

            for (int i = 0; i < pdaSlider.Length; i++)
            {
                if (i < 1 || i > 30)
                {
                    result += "000000";
                }
                else
                {
                    switch ((i-1)/5){
                        case 0:
                            result += ((MUS6Keys[0] == '1') ? Touchcolor : keyColors[0]);
                            break;
                        case 1:
                            result += ((MUS6Keys[1] == '1') ? Touchcolor : keyColors[1]);
                            break;
                        case 2:
                            result += ((MUS6Keys[2] == '1') ? Touchcolor : keyColors[2]);
                            break;
                        case 3:
                            result += ((MUS6Keys[3] == '1') ? Touchcolor : keyColors[3]);
                            break;
                        case 4:
                            result += ((MUS6Keys[4] == '1') ? Touchcolor : keyColors[4]);
                            break;
                        case 5:
                            result += ((MUS6Keys[5] == '1') ? Touchcolor : keyColors[5]);
                            break;
                    }
                }
            }
            return result;
        }

        public override void UpdateKeys(string pdaslider)
        {
            MUS6Keys = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 6; i++)
            {
                if (pdaslider[(i * 5) + 1] == '1' || pdaslider[(i * 5) + 2] == '1' || pdaslider[(i * 5) + 3] == '1' || pdaslider[(i * 5) + 4] == '1' || pdaslider[(i * 5) + 5] == '1')
                {
                    MUS6Keys += '1';
                }
                else
                {
                    MUS6Keys += '0';
                }
                if (LastMUS6Keys[i] != MUS6Keys[i])
                {
                    if (MUS6Keys[i] == '1')//0->1
                    {
                        win32api.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastMUS6Keys = MUS6Keys;
        }
    }

    /// <summary>
    /// MUSYNX 4Key
    /// </summary>
    class MUSYNX4 : slider
    {
        public override string Basecolor { get; set; } = "fefefe";
        public override string Touchcolor { get; set; } = "000000";
        private string[] keyColors =
        {
            "9af0d0",
            "96dbee",
            "be95fc",
            "e886f4"
        };

        private string MUS4Keys = "0000";
        private string LastMUS4Keys = "0000";

        public override string GetKeyState()
        {
            return MUS4Keys;
        }

        public override byte[] GameKeys { get; set; } =
        {
            //(byte)Keys.D1,//1
            //(byte)Keys.D2,
            //(byte)Keys.D3,
            //(byte)Keys.D4,
            //(byte)Keys.D5,
            //(byte)Keys.D6,
            //(byte)Keys.D7,
            //(byte)Keys.D8,
            //(byte)Keys.D9,
            //(byte)Keys.D0,
            //(byte)Keys.Q,
            //(byte)Keys.W,
            //(byte)Keys.E,
            //(byte)Keys.R,
            //(byte)Keys.T,
            //(byte)Keys.Y,
            //(byte)Keys.U,
            //(byte)Keys.I,
            //(byte)Keys.O,
            //(byte)Keys.P,
            //(byte)Keys.A,
            //(byte)Keys.S,
            (byte)Keys.D,
            (byte)Keys.F,
            //(byte)Keys.G,
            //(byte)Keys.H,
            (byte)Keys.J,
            (byte)Keys.K//,
            //byte)Keys.L//,
            //(byte)Keys.Z,
            //(byte)Keys.X,
            //(byte)Keys.C,
            //(byte)Keys.V,
            //(byte)Keys.B,
            //(byte)Keys.N,
            //(byte)Keys.M
        };

        public override string assembleTouchedSliderLED(string pdaSlider)
        {
            string result = "";

            for (int i = 0; i < pdaSlider.Length; i++)
            {
                switch (i / 8)
                {
                    case 0:
                        result += ((MUS4Keys[0] == '1') ? Touchcolor : keyColors[0]);
                        break;
                    case 1:
                        result += ((MUS4Keys[1] == '1') ? Touchcolor : keyColors[1]);
                        break;
                    case 2:
                        result += ((MUS4Keys[2] == '1') ? Touchcolor : keyColors[2]);
                        break;
                    case 3:
                        result += ((MUS4Keys[3] == '1') ? Touchcolor : keyColors[3]);
                        break;
                }
            }
            return result;
        }

        public override void UpdateKeys(string pdaslider)
        {
            MUS4Keys = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 4; i++)
            {
                if (pdaslider[i * 8] == '1'
                    || pdaslider[(i * 8) + 1] == '1' 
                    || pdaslider[(i * 8) + 2] == '1' 
                    || pdaslider[(i * 5) + 3] == '1' 
                    || pdaslider[(i * 5) + 4] == '1'
                    || pdaslider[(i * 5) + 5] == '1'
                    || pdaslider[(i * 5) + 6] == '1'
                    || pdaslider[(i * 5) + 7] == '1')
                {
                    MUS4Keys += '1';
                }
                else
                {
                    MUS4Keys += '0';
                }
                if (LastMUS4Keys[i] != MUS4Keys[i])
                {
                    if (MUS4Keys[i] == '1')//0->1
                    {
                        win32api.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastMUS4Keys = MUS4Keys;
        }
    }

    class SEGAToolsChuni : slider
    {
        public override string Basecolor { get; set; } = "fefefe";
        public override string Touchcolor { get; set; } = "000000";

        private string ChuniKeys = "00000000000000000000000000000000";
        private string LastChuniKeys = "00000000000000000000000000000000";

        public override string GetKeyState()
        {
            return ChuniKeys;
        }

        public override byte[] GameKeys { get; set; } =
        {
            (byte)Keys.D1,//1
            (byte)Keys.A,
            (byte)Keys.Q,
            (byte)Keys.Z,
            (byte)Keys.D2,
            (byte)Keys.S,
            (byte)Keys.W,
            (byte)Keys.X,
            (byte)Keys.D3,
            (byte)Keys.D,
            (byte)Keys.E,
            (byte)Keys.C,
            (byte)Keys.D4,
            (byte)Keys.F,
            (byte)Keys.R,
            (byte)Keys.V,
            (byte)Keys.D5,
            (byte)Keys.G,
            (byte)Keys.T,
            (byte)Keys.B,
            (byte)Keys.D6,
            (byte)Keys.H,
            (byte)Keys.Y,
            (byte)Keys.N,
            (byte)Keys.D7,
            (byte)Keys.J,
            (byte)Keys.U,
            (byte)Keys.M,
            (byte)Keys.D8,
            (byte)Keys.K,
            (byte)Keys.I,
            (byte)Keys.Oemcomma
        };

        public override string assembleTouchedSliderLED(string pdaSlider)
        {
            string result = "";

            for (int i = 0; i < pdaSlider.Length; i++)
            {
                if (pdaSlider[i] == '0')
                {
                    result += Basecolor;
                }
                else
                {
                    result += Touchcolor;
                }
            }
            return result;
        }

        public override void UpdateKeys(string pdaslider)
        {
            ChuniKeys = "";
            //forを使って1文字ずつ処理する
            for (int i = 0; i < 32; i++)
            {
                if (pdaslider[i] == '1')
                {
                    ChuniKeys += '1';
                }
                else
                {
                    ChuniKeys += '0';
                }
                if (LastChuniKeys[i] != ChuniKeys[i])
                {
                    if (ChuniKeys[i] == '1')//0->1
                    {
                        win32api.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        win32api.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastChuniKeys = ChuniKeys;
        }
    }

}
