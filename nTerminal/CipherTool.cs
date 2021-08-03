using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace CipherTool
{
    public static class Hash
    {
        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(txt);
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
        public static byte[] GenerateMD5bytes(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(txt);
                return mi.ComputeHash(buffer);
            }
        }
        public static string GenerateMD5(Stream inputStream)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] newBuffer = mi.ComputeHash(inputStream);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
    public sealed class AES256_CBC_PKCS7
    {
        public static String EncryptString(string Input, string Iv, string Key)
        {
            byte[] xBuff = EncryptBytes(Encoding.UTF8.GetBytes(Input), Encoding.UTF8.GetBytes(Iv), Encoding.UTF8.GetBytes(Key));
            return Convert.ToBase64String(xBuff);
        }

        public static String DecryptString(string Input, string Iv, string Key)
        {
            byte[] xBuff = DecryptBytes(Convert.FromBase64String(Input), Encoding.UTF8.GetBytes(Iv), Encoding.UTF8.GetBytes(Key));
            return Encoding.UTF8.GetString(xBuff);
        }
        public static byte[] EncryptBytes(byte[] Input, byte[] Iv, byte[] Key)
        {
            var aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            if (Iv == null)
            {
                aes.IV = new byte[16] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 };
            }
            else
            {
                aes.IV = Iv;
            }
            
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    cs.Write(Input, 0, Input.Length);
                }
                xBuff = ms.ToArray();
            }
            return xBuff;
        }

        public static byte[] DecryptBytes(byte[] Input, byte[] Iv, byte[] Key)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Key;
            if (Iv == null)
            {
                aes.IV = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            }
            else
            {
                aes.IV = Iv;
            }

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    cs.Write(Input, 0, Input.Length);
                }
                xBuff = ms.ToArray();
            }
            return xBuff;
        }
    }
}
