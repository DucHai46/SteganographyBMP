using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace SteganographyBMP
{
    class CryptoHelper
    {
        public static byte[] Encrypt(byte[] message,string password)
        {
            return CommonMethod(message, password);
        }

        public static byte[] Decrypt(byte[] message, string password)
        {
            return CommonMethod(message, password);
        }
        // Hàm dùng cho cả mã hoá lẫn giải mã
        private static byte[] CommonMethod(byte[] message, string password)
        {
            byte[] salt = {35, 1, 78};
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt); // thuật toán băm PBKDF1
            byte[] key = pdb.GetBytes(20);  // Chuyển đối tượng về dạng khoá có độ dài 20byte
            byte[] retMessage = new byte[message.Length];
            for (int i = 0; i < message.Length; i++)
            {
                int index = i % key.Length;     // Do chiều dài của message có thể lớn hơn chiều dài key
                retMessage[i] = (Byte)(key[index] ^ message[i]);    //Thực hiện phép XOR , mục đích là sau khi thực hiện phép XOR 2 lần sẽ được nguyên bản
            }

            return retMessage;  
        }
    }
}
