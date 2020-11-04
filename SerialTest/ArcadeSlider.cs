using DIVASliderTool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DIVASliderTool
{
    /// <summary>
    /// スライダー周り
    /// </summary>
    abstract class ArcadeSlider
    {
        public abstract string Basecolor { get; set; }
        public abstract string Touchcolor { get; set; }
        public abstract byte[] GameKeys { get; set; }
        public abstract void UpdateKeys(string pdaslider);
        public abstract string GetKeyState();

        Win32API Win32API = new Win32API();

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
    class Chunithm : ArcadeSlider
    {
        public override string Basecolor { get; set; } = "00fee6";
        public override string Touchcolor { get; set; } = "fefefe";
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
                        Win32API.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        Win32API.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastUniSlider = UniSlider;
        }
    }

    /// <summary>
    /// Nostalgia
    /// </summary>
    class Nostalgia : ArcadeSlider
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

            for (int i = 0; i < pdaSlider.Length; i++)
            {
                if (i < 3 || i > 28)
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
                if (LastNostalKeys[i] != NostalKeys[i] && 1 < i && i < 29)
                {
                    if (NostalKeys[i] == '1')//0->1
                    {
                        Win32API.keybd_event(GameKeys[i - 2], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        Win32API.keybd_event(GameKeys[i - 2], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastNostalKeys = NostalKeys;
        }
    }

    /// <summary>
    /// MUSYNX 6Key
    /// </summary>
    class MUSYNX6 : ArcadeSlider
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
                    switch ((i - 1) / 5)
                    {
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
                        Win32API.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        Win32API.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastMUS6Keys = MUS6Keys;
        }
    }

    /// <summary>
    /// MUSYNX 4Key
    /// </summary>
    class MUSYNX4 : ArcadeSlider
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
                        Win32API.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        Win32API.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastMUS4Keys = MUS4Keys;
        }
    }

    class SEGAToolsChuni : ArcadeSlider
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
                        Win32API.keybd_event(GameKeys[i], 0, 0, (UIntPtr)0);
                    }
                    else//1->0
                    {
                        Win32API.keybd_event(GameKeys[i], 0, 2, (UIntPtr)0);//(byte)win32api.MapVirtualKey(keys[i], 3)
                    }
                }
            }
            LastChuniKeys = ChuniKeys;
        }
    }

}