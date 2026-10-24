using game.Entity.Notify;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace game.WebSite
{
    public static class SMS
    {
        /// <summary>
        /// 取得簡訊資料
        /// </summary>
        /// <param name="codeID"></param>
        /// <returns></returns>
        public static SMSVCode GetSMS(string codeID)
        {
            var result = SiteHelper.GetAPIResult<SMSVCode>(string.Format("/Notify/GetSMS?codeID={0}", codeID));
            return result;
        }
        
        public static void UpdateSendResult(string codeID, bool isSuccess, string sendResult)
        {
            //1 發送成功, 2發送失敗
            string status = isSuccess ? "1" : "0";
            SiteHelper.GetAPIResult(string.Format("/Notify/UpdateSMSSendResult?codeID={0}&status={1}&result={2}", codeID, status, sendResult));
        }

        public static string GetPostData_submail(string vCode, string tel)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["appid"] = Submail_Appid;
            dic["to"] = tel;
            dic["project"] = Submail_Project;
            dic["signature"] = Submail_Signature;
            dic["vars"] = "{\"code\":\"" + vCode + "\"}";
            return string.Join("&", dic.Select(d => string.Format("{0}={1}", d.Key, d.Value)));
        }

        public static string GetPostData_ucpaas(string vCode, string tel)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["appid"] = UCPaas_Appid;
            dic["token"] = UCPaas_Token;
            dic["sid"] = UCPaas_Sid;
            dic["templateid"] = UCPaas_Templateid;
            dic["param"] = vCode;
            dic["mobile"] = tel;
            return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
        }


        public static string SubmailAPI
        {
            get { return ConfigurationManager.AppSettings["submail_url"].ToString();  }
        }

        public static string Submail_Appid
        {
            get { return ConfigurationManager.AppSettings["submail_appid"].ToString(); }
        }

        public static string Submail_Signature
        {
            get { return ConfigurationManager.AppSettings["submail_signature"].ToString(); }
        }

        public static string Submail_Project
        {
            get { return ConfigurationManager.AppSettings["submail_project"].ToString(); }
        }

        public static string UCPaasAPI
        {
            get { return ConfigurationManager.AppSettings["ucpaas_url"].ToString(); }
        }

        public static string UCPaas_Appid
        {
            get { return ConfigurationManager.AppSettings["ucpaas_appid"].ToString(); }
        }

        public static string UCPaas_Token
        {
            get { return ConfigurationManager.AppSettings["ucpaas_token"].ToString(); }
        }

        public static string UCPaas_Sid
        {
            get { return ConfigurationManager.AppSettings["ucpaas_sid"].ToString(); }
        }

        public static string UCPaas_Templateid
        {
            get { return ConfigurationManager.AppSettings["ucpaas_templateid"].ToString(); }
        }

        public static string Logname { get { return "sms.log"; } }

    }
}