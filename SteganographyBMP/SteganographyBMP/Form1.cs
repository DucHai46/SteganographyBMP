using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace SteganographyBMP
{    
    public partial class Form1 : Form
    {
        private string inPath1;       //Đường dẫn tới ảnh cần giấu
        
        private string inPath2;       //Đường dẫn tới ảnh cần giải mã

            // dùng để sinh key



        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            this.Dispose();
        }

        private void FitPicture(Image image,PictureBox frame,GroupBox box)
        {
            box.Width = image.Width <512 ? image.Width : 512;
            frame.Width = image.Width<512 ? image.Width : 512;
            box.Height = image.Height<512 ? image.Height : 512;
            frame.Height = image.Height<512 ? image.Height : 512 ;
        }
        private void openImageToHidingToolStripMenuItem_Click(object sender, EventArgs e)
        {           
            OpenFileDialog openFileDialog1=new OpenFileDialog();
            openFileDialog1.Title = "Chose an Bitmap Image To Hiding";
            openFileDialog1.Filter = "Bitmap Image(*.bmp)|*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                inPath1 = openFileDialog1.FileName; // Lấy đường dẫn của file vừa mở
            }
            else
            {
                inPath1 = "";
            }
            Bitmap oldBitmap = new Bitmap(inPath1);            
            FitPicture(oldBitmap,pictureBox1,groupBox1);
            pictureBox1.Image = oldBitmap;            
        }

        private byte[] AddMessLengthToAhead(byte[] message)
        {
            int messLen = message.Length;
            byte[] byteLen = BitConverter.GetBytes(messLen);    // Chuyển độ dài thông điệp thành các mảng byte, thực chất chỉ cần 4 byte là đủ;
            byte[] newMess = new byte[messLen + byteLen.Length];    //Thông  điệp mới có độ dài bằng độ dài thông điệp + độ dài mảng byte
            for (int i = 0; i < byteLen.Length; i++)
                newMess[i] = byteLen[i];
            for (int i = 0; i < messLen; i++)
                newMess[i + byteLen.Length] = message[i];
            return newMess;
        }

        //Tạo 1 file stego từ đầu vào
        public void CreateStegoFile(string inPath1, string message, string password, string inPath2)
        {
            //Kiểm tra đầu vào
            if (inPath1 == "") 
                throw new Exception("Ban chua chon anh de giau tin");
            if (inPath2 == "")
                throw new Exception("Ban chua chon noi de luu anh");
            if (message == "")
                throw new Exception("Ban chua nhap thong diep de giau");
            if (password == "")
                throw new Exception("Ban chua nhap mat khau");
            // Mở file đầu vào
            FileStream inStream = new FileStream(inPath1, FileMode.Open, FileAccess.Read);
            
            // kiểm tra có phải ảnh bitmap không
            char b = (char)inStream.ReadByte();// bit thứ nhất và thứ hai để định dạng kiểu tệp
            char m = (char)inStream.ReadByte();
            if (!(b == 'B' && m == 'M'))
                throw new Exception("Khong phai la anh bitmap");
            //Kiểm tra có phải ảnh 24 bit hay không
            inStream.Seek(28,0);    // Đưa con trỏ về vị trí byte thứ 28
            byte[] temp=new byte[2];
            inStream.Read(temp,0,2);    // số bit/1pixel được lưu bằng 2 byte
            Int16 nBit= BitConverter.ToInt16(temp,0);       // chuyển mảng temp về số nguyên 16bit
            if (nBit != 24)
                throw new Exception("Day khong phai la anh 24 bit");
            // Đọc 54 bit phần header (bitmap header + bitmap infor) đưa vào trong outStream
            int offset =54;
            inStream.Seek(0, 0);
            byte[] header = new byte[offset];
            inStream.Read(header, 0, offset);

            //Ghi 54 bit vào file stego (file đầu ra)
            FileStream outStream = new FileStream(inPath2, FileMode.Create, FileAccess.Write);
            outStream.Write(header, 0, offset);
            
            // Mã hoá thông điệp và mật khẩu thành 1 thông điệp duy nhất
            UnicodeEncoding unicode = new UnicodeEncoding();
            byte[] newMessageByte = AddMessLengthToAhead(unicode.GetBytes(message));// Thêm 4 byte độ dài của message vào đầu thông điệp
            // Thực hiện trộn
            newMessageByte = CryptoHelper.Encrypt(newMessageByte, password);

            // Thực hiện giấu tin vào trong ảnh
            inStream.Seek(offset, 0);    // Đưa con trỏ tới nơi bắt đầu của data tại vị trí thứ 54
            LSBHelper.Encode(inStream, newMessageByte, outStream);      // Thay từng bit trong thông điệp vào LSB của inStream và ghi vào outStream

            inStream.Close(); // Đóng file ảnh
            outStream.Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Mở hộp thoại để chọn vị trí lưu file stego
            SaveFileDialog saveDialog1 = new SaveFileDialog();
            saveDialog1.Title="Where do you want to save the file?";
            saveDialog1.Filter = "Bitmap (*.bmp)|*.bmp";

            string outPath;
            if (saveDialog1.ShowDialog() == DialogResult.OK)
            {
                outPath = saveDialog1.FileName;         
            }
            else
            {
                outPath = "";
            }
            // Tạo file stego chứa thông điệp
            CreateStegoFile(inPath1, textBox1.Text, textBox2.Text, outPath);
            // Đưa ảnh chứa thông điệp lên picturebox
            Bitmap bitmap = new Bitmap(outPath);
            FitPicture(bitmap, pictureBox2, groupBox2);
            pictureBox2.Image = bitmap;
            saveDialog1.Dispose();
        }

        
        private void openImageToExtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "Chose an Image To Extract";
            openFileDialog1.Filter = "Bitmap (*.bmp)|*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                inPath2 = openFileDialog1.FileName;
            }
            else { inPath2 = ""; }
            // Load ảnh lên picturebox 3
            Bitmap bitmap = new Bitmap(inPath2);
            FitPicture(bitmap,pictureBox3,groupBox3);
            pictureBox3.Image = bitmap;

        }

        private void button3_Click(object sender, EventArgs e)  // Thực hiện trích thông điệp từ file stego
        {
            // Kiểm tra đầu vào có hợp lệ không
            if (inPath2 == "")
                throw new Exception("Ban chua chon file anh de trich thong tin");
            string password = textBox3.Text;
            if (password == "")
                throw new Exception("Ban chua nhap mat khau");
            
            FileStream inStream = new FileStream(inPath2, FileMode.Open, FileAccess.Read);
            inStream.Seek(0, 0);    
            char b = (char)inStream.ReadByte();
            char m = (char)inStream.ReadByte();
            if (!(b == 'B' && m == 'M'))
                throw new Exception("Day khong phai la file Bitmap");
            
            int offset = 28;
            inStream.Seek(offset,0);
            byte[] temp=new byte[2];    
            inStream.Read(temp,0,2);    
            
            Int16 numOfBit = BitConverter.ToInt16(temp, 0);
            if (numOfBit != 24)
                throw new Exception("Khong phai anh 24 bit");
            // Bắt đầu giải mã
            offset = 54;
            inStream.Seek(offset, 0);
            byte[] bLen = new byte[4];  // 4 byte lưu độ dài thông điệp
            bLen = LSBHelper.Decode(inStream, 4);   // Không thể dùng FileStream.Read, vì 4 byte này bản chất là bit đầu của mỗi 32 byte = 32bit(4byte) trong inStream
            //decrypt 4 byte này để được 4 byte thực sự ban đầu (do 4 byte này cũng dược mã hoá bởi key[128])
            bLen = CryptoHelper.Decrypt(bLen, password);
            int length = BitConverter.ToInt32(bLen, 0); // Chuyển từ mảng byte về số nguyên

            // Đọc ra mảng thông điệp ẩn
            inStream.Seek(offset + 4 * 8, 0);       //  32 byte đầu tiên để lưu độ dài thông điệp
            byte[] buffer=new byte[length];         // dùng để lưu trữ tạm thời
            try
            {
                buffer = LSBHelper.Decode(inStream, length);
            }
            catch { throw new Exception("Anh nay khong chua thong tin  hoac ban da nhap sai mat khau"); } 
            byte[] realHidenMessage = new byte[4 + buffer.Length];
            realHidenMessage = ConcatTwoByteArray(bLen, buffer);   // Thêm 4 byte vào đầu để tiện cho việc Decrypt
            realHidenMessage = CryptoHelper.Decrypt(realHidenMessage, password);  // Thông điệp thật sự được giấu
            byte[] hidenMessage = new byte[length];
            for (int i = 0; i < length; i++)
                hidenMessage[i] = realHidenMessage[i + 4];
            // chuyển về mảng String
            UnicodeEncoding unicode = new UnicodeEncoding();
            string result = unicode.GetString(hidenMessage);
            textBox4.Text = result;

            inStream.Close();
        }

        //Nối 2 mảng byte lại với nhau
        private byte[] ConcatTwoByteArray(byte[] arr1, byte[] arr2)
        {
            byte[] retArr = new byte[arr1.Length + arr2.Length];

            for (int i = 0; i < arr1.Length; i++)
                retArr[i] = arr1[i];

            for (int i = 0; i < arr2.Length; i++)
                retArr[i + arr1.Length] = arr2[i];
            return retArr;
        }

    }
}
