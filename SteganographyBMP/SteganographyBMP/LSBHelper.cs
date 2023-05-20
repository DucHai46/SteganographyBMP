using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SteganographyBMP
{
    class LSBHelper
    {
        public static void Encode(FileStream inStream, byte[] message, FileStream outStream)
        {
            int byteRead;      
            byte byteWrite;     
            int i = 0;          // chạy trong mảng byte message
            int j = 0;          // chạy từng bit trong 1 byte message[i]
            while ((byteRead = inStream.ReadByte()) != -1)
            {
                byteWrite = (byte)byteRead;

                if (i < message.Length)       // Nếu thông điệp vẫn còn
                {
                    byte bit = BitWise.Extract(message[i], j++);   // Trích 1 bit từ vị trí j từ message[i] ra
                    BitWise.Replace(ref byteWrite, 0, bit);            // Thay thế bit vào vị trí 0 (LSB)                                 
                    if (j == 8) { j = 0; i++; }    // Đã trích hết 8 bit của message[i]
                }
                //viết ra outStream(Có trường hợp byte cuối không bị thay đổi)
                outStream.WriteByte(byteWrite);
            }

            if (i < message.Length) //i chưa chạy hết mảng message[]
                throw new Exception("Thong diep qua lon de giau");
        }

        public static byte[] Decode(FileStream inStream,int length) // Đọc ra 1 mảng byte[] có chiều dài length trong file stego
        {
            int byteIndex = 0;
            int bitIndex = 0;
            byte[] arrResult=new byte[length];
            int byteRead;   
            while ((byteRead = inStream.ReadByte()) != -1)
            {
                byte bit = BitWise.Extract((byte)byteRead, 0);  // lấy ra 1 bit tại vị trí 0
                //Thay thế bit lấy được vào trong byte result[byteIndex] hiện tại, tại vị trí bitIndex
                BitWise.Replace(ref arrResult[byteIndex], bitIndex++, bit);
                if (bitIndex == 8)    // Thu được 1 byte
                {                    
                    bitIndex = 0;
                    byteIndex++;
                }
                if (byteIndex == length) break; // Đủ nội dung thông điệp
            }
            return arrResult;
        }
    }
}
