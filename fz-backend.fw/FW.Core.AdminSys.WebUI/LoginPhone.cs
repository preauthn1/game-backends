using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Web;

namespace FW.Core.AdminSys.WebUI
{
    //处理手机验证码登录

    internal static class AdminPhone
    {
        private static string _smsConfig = string.Empty;

        static AdminPhone()
        {             
            try{
                _smsConfig = ConfigurationManager.AppSettings["Admin_SmsConfig"];
            }
            catch{}
        }
        

        public static string Random(int length)
        {
            char[] arrChar = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder num = new StringBuilder();
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < length; i++)
            {
                num.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());
            }
            return num.ToString();
        }

        /// <summary>
        /// 获取配置的手机验证码失效时间（分钟) 默认180分钟
        /// </summary>
        /// <returns></returns>
        public static int ExpiredTime()
        {
            var config = _smsConfig.Split('|');

            if (config.Length >= 4) {
                try
                {
                    return Convert.ToInt32(config[3]);
                }
                catch {
                    return 180;
                }
            }
            return 180;
        }

        /// <summary>
        /// 向手机发送验证码
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="vCode"></param>
        /// <returns></returns>
        public static void Send(string phone, string vCode) {
            var config = _smsConfig.Split('|');
            string param = string.Format("action=send&userid={0}&account={1}&password=[REDACTED]", config[0], config[1], config[2]
                , string.Format("验证码:{0} 请您尽快登录!", vCode), phone);

            byte[] bs = Encoding.UTF8.GetBytes(param);

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("http://0.0.0.0:9999/sms.aspx");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse wr = req.GetResponse())
            {
                StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.Default);
            }
        }

    }


    internal static class AdminSafeIP {
        
        private static List<string> _safeIP = new List<string>();

        static AdminSafeIP() { 
            string config = string.Empty;
            try{
                config =  ConfigurationManager.AppSettings["Admin_SafeIP"];
            }
            catch{}

            foreach (string item in config.Split('|')) {
                if (string.IsNullOrEmpty(item) == false) {
                    _safeIP.Add(item);
                    
                }
            }
        }

        /// <summary>
        /// 当前IP
        /// </summary>
        /// <returns></returns>
        public static string CurrentIP() {
            return IPHelper.GetIP();
        }

        /// <summary>
        /// 是否包含指定IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool Contains(string ip) {
            if (_safeIP.Contains("*")) return true;

            return _safeIP.Contains(ip);
        }


    }


}