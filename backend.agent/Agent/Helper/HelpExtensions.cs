using System;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;

namespace Agent.Helper
{
    public static class HelpExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            if (value == null) return "N/A";

            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GetEnumDescription(this int value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        /// 显示金币格式
        /// </summary>
        /// <param name="_this"></param>
        /// <returns></returns>
        public static string ToCash(this long _this)
        {
            float vOut = Convert.ToSingle(_this);
            return (vOut / 100).ToString("N2");
        }

        public static bool CheckPasswordMD5(string password, string source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            using (MD5 md5Hash = MD5.Create())
            {
                string hash = verification.GetMd5Hash(md5Hash, source);
                if (!password.Equals(hash))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>產生亂數字串</summary>
        /// <param name="Number">字元數</param>
        /// <returns></returns>
        public static string CreateRandomCode(int Number)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;

            Random rand = new Random();
            for (int i = 0; i < Number; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(36);
                if (temp != -1 && temp == t)
                {
                    return CreateRandomCode(Number);
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }



        /// <summary>
        /// 取得使用者IP
        /// </summary>
        /// <returns></returns>
        public static string GetIP(HttpRequestBase request)
        {
            string ip;
            string trueIP = string.Empty;

            //先取得是否有經過代理伺服器
            ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ip))
            {
                //將取得的 IP 字串存入陣列
                string[] ipRange = ip.Split(',');

                //比對陣列中的每個 IP
                for (int i = 0; i < ipRange.Length; i++)
                {
                    //剔除內部 IP 及不合法的 IP 後，取出第一個合法 IP
                    if (ipRange[i].Trim().Substring(0, 3) != "10." &&
                        ipRange[i].Trim().Substring(0, 5) != "127.0" &&
                        ipRange[i].Trim().Substring(0, 7) != "192.168" &&
                        ipRange[i].Trim().Substring(0, 7) != "172.16." &&
                        CheckIP(ipRange[i].Trim()))
                    {
                        trueIP = ipRange[i].Trim();
                        break;
                    }
                }

            }
            else
            {
                //沒經過代理伺服器，直接使用 ServerVariables["REMOTE_ADDR"]
                //並經過 CheckIP( ) 的驗證
                trueIP = CheckIP(request.ServerVariables["REMOTE_ADDR"]) ?
                    request.ServerVariables["REMOTE_ADDR"] : "";
            }

            return trueIP;
        }


        /// <summary>
        /// 檢查 IP 是否合法
        /// </summary>
        public static bool CheckIP(string strPattern)
        {
            Regex regex = new Regex("^\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}[\\.]\\d{1,3}$");
            Match m = regex.Match(strPattern);
            return m.Success;
        }


        public static string getConfig(string key)
        {
            return ConfigurationManager.AppSettings.Get(key);
        }
    }
}