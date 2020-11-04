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

        SegaSerialHelper SerialHelper = new SegaSerialHelper();

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


        public Form1()
        {
            InitializeComponent();
        }

        //slider slider = new Chunithm();
        ArcadeSlider[] sliders =
        {
            new Chunithm(),
            new Chunithm(),
            new Nostalgia(),
            new MUSYNX4(),
            new MUSYNX6(),
            new SEGAToolsChuni()
        };

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                portComboBox.Items.Add(port);
            }
            foreach (string command in SerialHelper.commands)
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

        private void openPort1()
        {
            serialPort1.BaudRate = 115200;
            serialPort1.Parity = Parity.None;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.Handshake = Handshake.None;
            serialPort1.PortName = portComboBox.Text;
            serialPort1.Open();
        }

        private void openPort2()
        {
            serialPort2.BaudRate = 115200;
            serialPort2.Parity = Parity.None;
            serialPort2.DataBits = 8;
            serialPort2.StopBits = StopBits.One;
            serialPort2.Handshake = Handshake.None;
            serialPort2.PortName = "COM10";
            serialPort2.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openPort1();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                byte[] buffer = SerialHelper.hexStringToBytes(TXcomboBox.Text);
                serialPort1.Write(buffer, 0, buffer.Length);
            }
        }

        private void sendPacket(string strPacket)
        {
            sending = true;
            strPacket += SerialHelper.calcSUM(strPacket).ToString("X2");
            byte[] packet = SerialHelper.hexStringToBytes(strPacket);
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
            if (tmp == 100 && len == 18)
            {
                chkSum = new byte[3];
                chkSum[0] = (byte)tmp;
                chkSum[1] = (byte)serialPort1.ReadByte();
                chkSum[2] = (byte)serialPort1.ReadByte();
            }
            else if (tmp == 253)
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
            Response("mySUM:" + SerialHelper.calcSUM(SerialHelper.bytesToString(header) + SerialHelper.bytesToString(data)).ToString("X2"));

            //スキャンデータが来たらキー入力に変換&LEDに反映
            if (header[1] == 0x01)
            {
                string pdaslider = readSliderPacket(data);
                sliders[mode].UpdateKeys(pdaslider);
                if (mode != 5) sendPacket(SerialHelper.commands[3] + sliders[mode].assembleTouchedSliderLED(pdaslider));
                sliderResponse(sliders[mode].GetKeyState());
            }

        }

        private void serialPort2_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] LEDData = new byte[100];
            if (LEDData[0] != 0xAA)
            {
                while (LEDData[0] != 0xAA) LEDData[0] = (byte)serialPort2.ReadByte();
            }

            for (int i = 1; i < 100; i++)
            {
                LEDData[i] = (byte)serialPort2.ReadByte();
                //if (LEDData[i] == 0xff) return;
            }
            string strData = BitConverter.ToString(LEDData).Replace("-", string.Empty);
            if (debug) Response("LEDdata:" + strData);
            if (debug) Response("LEDPacket:" + SerialHelper.commands[3] + strData.Substring(4, 192));
            string[] LEDArr = strData.Substring(4, 192).SubstringAtCount(6);
            Array.Reverse(LEDArr);
            string LEDPacket = "";
            foreach (string LEDRGB in LEDArr)
            {
                LEDPacket += LEDRGB;
            }
            sendPacket(SerialHelper.commands[3] + LEDPacket);

        }

        private string readSliderPacket(byte[] datas)
        {
            string pdaslider = "";
            int j;
            foreach (byte data in datas)
            {
                if (data != 0) { j = 1; } else { j = 0; }
                pdaslider += j;
            }
            return pdaslider;
        }



        private void button3_Click(object sender, EventArgs e)
        {
            if (!sending)
            {
                serialPort1.Close();
            }
        }

        private void LEDSendButton_Click(object sender, EventArgs e)
        {
            string packet = SerialHelper.commands[3];
            for (int i = 0; i < 32; i++)
            {
                packet += LEDTextBox.Text;
            }
            packet += SerialHelper.calcSUM(packet).ToString("X2");
            sendPacket(packet);
            if (debug) Response("Send:" + packet);

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            sendPacket(SerialHelper.commands[2]);
        }

        private void scanButtonClicked(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void scanStopButton_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                byte[] buffer = SerialHelper.hexStringToBytes(SerialHelper.commands[4]);
                serialPort1.Write(buffer, 0, buffer.Length);
            }
        }

        private void logCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            debug = logCheckBox1.Checked;
        }

        private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            mode = ModeComboBox.SelectedIndex;
            if (mode == 5)
            {
                openPort2();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label1.Text = "SUM: " + SerialHelper.calcSUM(TXcomboBox.Text).ToString("x2");
        }
    }


    public static class StringExtensions
    {
        public static string[] SubstringAtCount(this string self, int count)
        {
            var result = new List<string>();
            var length = (int)Math.Ceiling((double)self.Length / count);

            for (int i = 0; i < length; i++)
            {
                int start = count * i;
                if (self.Length <= start)
                {
                    break;
                }
                if (self.Length < start + count)
                {
                    result.Add(self.Substring(start));
                }
                else
                {
                    result.Add(self.Substring(start, count));
                }
            }

            return result.ToArray();
        }
    }
}