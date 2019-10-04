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

namespace SerialTest
{
    public partial class Form1 : Form
    {
        string[] commands =
        {
            //コマンド一覧
            //[ヘッダ(0xff固定値)][コマンドコード][実データ長][実データ][sum]
            //
            "FF1000F1",//接続確認
            "FFF00011",//デバイス情報
            "FF0300FE",//スキャン開始
            "FF02613F",//LED点灯ヘッダ＋ [ ([blue] [red] [green]) x 32] [sum]
            //LEDダンプデータ？
            "ff02613f22fb2831e83e43d05756b87068a0897b89a18d71baa059d3b241eca454ee9469ee837fee7395ee62abee52c0ee41d6ee31ecee33efdc37f1ca3bf3b73ff5a443f79147f97f4bfb6c4ffdfc5964e45579c8528fac4fa5914cbb7549d05946e63d4368ff02613f22fa2b35e24447ca5d5ab2766c9b8e7f83a7916bc0a453d9b143eea059ee906eee7f84ee6f9aee5eb0ee4ec5ee3ddbee30eeea34f0d838f2c53cf4b240f69f44f88d48fa7a4cfc6753f95769dd547ec25194a64eaa8a4bc06e48d55345eb374269ff02613f27f43139dc4a4cc5635ead7c719594837dad9666c6a84edfad48ee9c5eee8c74ee7b89ee6b9fee5ab5ee4acbee39e0ee31eee635f0d339f2c13df4ae41f69b45f88849fa764dfc6358f2566ed75384bb50999f4daf834ac56847db4c44f0304165"
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

            byte[] data = new byte[len + 1];
            byte myChkSum = (byte)((header[1] + header[2])%255);
            if (len != 0)
            {
                for (int i = 0; i < len; i++)
                {
                    data[i] = (byte)serialPort1.ReadByte();
                    myChkSum = (byte)((myChkSum + data[i])%255);
                }
                string strData = BitConverter.ToString(data).Replace("-", string.Empty);
                Response("data:" + strData);
            }

            //sum読み取り
            byte[] chkSum = new byte[3];
            chkSum[0] = (byte)serialPort1.ReadByte();
            int tmp = chkSum[0];
            if(tmp==100 && len==18)
            {
                chkSum[1] = (byte)serialPort1.ReadByte();
                chkSum[2] = (byte)serialPort1.ReadByte();
            }
            if (tmp == 253)
            {
                chkSum[1] = (byte)serialPort1.ReadByte();
            }
            string strSum = BitConverter.ToString(chkSum).Replace("-", string.Empty);
            Response("sum:" + strSum);


            //checksum
            
            Response("calc sum:" + myChkSum.ToString("X2"));
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
    }
}
