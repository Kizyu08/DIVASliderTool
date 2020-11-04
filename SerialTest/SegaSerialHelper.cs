using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIVASliderTool
{

    class SegaSerialHelper
    {
        public string[] commands =
        {
            //コマンド一覧
            //[ヘッダ(0xff固定値)][コマンドコード][実データ長][実データ][sum]
            //
            "FF1000F1",//0接続確認
            "FFF00011",//1デバイス情報
            "FF0300FE",//2スキャン開始
            "FF02613F",//3LED点灯ヘッダ＋ [ ([blue] [red] [green]) x 32] [sum]
            //LEDダンプデータ
            "ff02613f27f43139dc4a4cc5635ead7c719594837dad9666c6a84edfad48ee9c5eee8c74ee7b89ee6b9fee5ab5ee4acbee39e0ee31eee635f0d339f2c13df4ae41f69b45f88849fa764dfc6358f2566ed75384bb50999f4daf834ac56847db4c44f0304165",
            "FF0400FDFC"//4スキャン停止
        };

        /// <summary>
        /// 16進数文字列をbyte[]に
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public byte[] hexStringToBytes(string str)
        {
            int length = str.Length / 2;
            byte[] bytes = new byte[length];
            int j = 0;
            for (int i = 0; i < length; i++)
            {
                bytes[i] = Convert.ToByte(str.Substring(j, 2), 16);
                j += 2;
            }
            return bytes;
        }

        /// <summary>
        /// byte[]を16進数文字列に
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string bytesToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        /// <summary>
        /// チェックサムを計算
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public byte calcSUM(string packet)
        {
            int sum = 0;
            byte[] bytesPacket = hexStringToBytes(packet);
            if (bytesPacket.Length != 0)
            {
                for (int i = 0; i < bytesPacket.Length; i++)
                {
                    sum += bytesPacket[i];
                }
                sum = 0x100 - (sum & 0xFF);
            }
            return (byte)sum;
        }
    }
}
