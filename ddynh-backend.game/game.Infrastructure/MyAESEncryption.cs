using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    public class MyAESEncryption
    {
        //預設IV值
        private static byte[] _IV = Encoding.ASCII.GetBytes("DNW123EXGMTR83K7");

        public static void SetIV(string s)
        {
            _IV = Encoding.ASCII.GetBytes(s);
        }
        /// <summary>  
        /// AES加密算法  
        /// </summary>  
        public static byte[] AESEncrypt(string plainText, string strKey)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);

            aes.Key = Encoding.UTF8.GetBytes(strKey);
            aes.IV = _IV;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            byte[] cipherBytes = ms.ToArray();
            cs.Close();
            ms.Close();
            return cipherBytes;
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        public static byte[] AESDecrypt(byte[] cipherText, string strKey)
        {
            SymmetricAlgorithm aes = Rijndael.Create();
            aes.Key = Encoding.UTF8.GetBytes(strKey);
            aes.IV = _IV;
            byte[] decryptBytes = new byte[cipherText.Length];
            MemoryStream ms = new MemoryStream(cipherText);
            CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            cs.Read(decryptBytes, 0, decryptBytes.Length);
            cs.Close();
            ms.Close();
            return decryptBytes;
        }

        public static byte[] HexToByte(string value)
        {
            //運算後的位元組長度:16進位數字字串長/2
            byte[] byteOUT = new byte[value.Length / 2];
            for (int i = 0; i < value.Length; i = i + 2)
            {
                //每2位16進位數字轉換為一個10進位整數
                byteOUT[i / 2] = Convert.ToByte(value.Substring(i, 2), 16);
            }
            return byteOUT;
        }

        public static string ByteToHex(byte[] value)
        {
            return BitConverter.ToString(value).Replace("-", "");
        }
    }
}
