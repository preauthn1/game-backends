using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;

namespace game.WebSite
{
    public static class MD5Service
    {
        /// <summary>
        /// MD5 加密
        /// </summary>
        public static string Hash(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            //md5 16bit
            //return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str)), 4, 8).Replace("-", "");
            //md5 32bit
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        public static string Hash(string str)
        {
            return Hash(str, Encoding.UTF8);
        }

        public static string UpperCaseUrlEncode(string urlEncodeString)
        {
            char[] temp = HttpUtility.UrlEncode(urlEncodeString).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++)
            {
                if (temp[i] == '%')
                {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }
    }
}