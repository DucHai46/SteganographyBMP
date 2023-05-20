using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteganographyBMP
{
    class BitWise
    {
        public static byte Extract(byte B, int pos)     // trích 1 bit từ byte B tại vị trí pos
        {
            return (byte)((B >> pos) & 1);
        }

        public static void Replace(ref byte B, int pos, byte value) //Thay đổi giá trị của một bit cụ thể trong một biến byte
        {
           B= (byte) (value == 1 ? B | (1 << pos) : B & ~(1 << pos));
        }
    }
}
