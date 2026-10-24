using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Service.Helper
{
    public static class HelpExtensions
    {
        public static bool CheckPasswordMD5(string password, string source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            
            using (MD5 md5Hash = MD5.Create())
            {               
                string hash = Verification.GetMd5Hash(md5Hash, source);
                if (password != hash)
                {
                    return false;
                }
                return true;
            }
        }

        public static string ToMD5(this string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = System.Text.Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", String.Empty);

                return md5;
            }
        }
    }
}