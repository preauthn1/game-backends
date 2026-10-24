using game.Entity.Notify;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System;
using Newtonsoft.Json.Linq;

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

        //
        public static SMSVenderInfo GetVenderInfo(int userId, string vender)
        {
            var result = SiteHelper.GetAPIResult<List<SMSVenderInfo>>($"/Notify/GetSMSVenderInfo?userId={userId}&vender={vender}");
            return result[0];
        }

        public static void UpdateSendResult(string codeID, bool isSuccess, string sendResult)
        {
            //1 發送成功, 2發送失敗
            string status = isSuccess ? "1" : "0";
            SiteHelper.GetAPIResult(string.Format("/Notify/UpdateSMSSendResult?codeID={0}&status={1}&result={2}", codeID, status, sendResult));
        }

        #region Submail
        public static string GetPostData_submail(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
            {
                return string.Empty;
            }
            JObject json = JObject.Parse(venderInfo.SMSInfo);

            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic["appid"] = json["appid"] != null ? json["appid"].ToString() : string.Empty;
            dic["to"] = tel;
            dic["project"] = json["project"] != null ? json["project"].ToString() : string.Empty;
            dic["signature"] = json["signature"] != null ? json["signature"].ToString() : string.Empty;
            dic["vars"] = "{\"code\":\"" + vCode + "\"}";
            //return string.Join("&", dic.Select(d => string.Format("{0}={1}", d.Key, d.Value)));
            return string.Join("&", dic.Select(d => $"{d.Key}={d.Value}"));
        }

        public static string SubmailAPI
        {
            get { return ConfigurationManager.AppSettings["submail_url"].ToString(); }
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
        #endregion

        #region UCPass
        public static string GetPostData_ucpaas(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
            {
                return string.Empty;
            }
            JObject json = JObject.Parse(venderInfo.SMSInfo);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic["appid"] = json["appid"] != null ? json["appid"].ToString() : string.Empty;
            dic["token"] = json["token"] != null ? json["token"].ToString() : string.Empty;
            dic["sid"] = json["sid"] != null ? json["sid"].ToString() : string.Empty;
            dic["templateid"] = json["templateid"] != null ? json["templateid"].ToString() : string.Empty;
            dic["param"] = vCode;
            dic["mobile"] = tel;
            return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
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
        #endregion

        #region Haoservice
        public static string GetPostData_haoservice(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
                {
                    return string.Empty;
                }
                JObject json = JObject.Parse(venderInfo.SMSInfo);

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["mobile"] = tel;
                dic["tpl_id"] = json["templateid"] != null ? json["templateid"].ToString() : string.Empty;
                dic["tpl_value"] = HttpUtility.UrlEncode(String.Format("#code#={0}", vCode));
                dic["key"] = json["apikey"] != null ? json["apikey"].ToString() : string.Empty;
                var returnStr = string.Join("&", dic.Select(m => m.Key + "=" + m.Value).ToArray());
                return returnStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        public static string HaoServiceAPI
        {
            get
            {
                var url = ConfigurationManager.AppSettings["haoservice_url"].ToString();
                if (ConfigurationManager.AppSettings["haoservice_method"].ToString() == "GET")
                {
                    url += "?";
                }
                return url;
            }
        }

        public static string HaoService_Method
        {
            get { return ConfigurationManager.AppSettings["haoservice_method"].ToString(); }
        }

        public static string HaoService_Aapikey
        {
            get { return ConfigurationManager.AppSettings["haoservice_apikey"].ToString(); }
        }

        public static string HaoService_Templateid
        {
            get { return ConfigurationManager.AppSettings["haoservice_templateid"].ToString(); }
        }

        public static string HaoService_Templatekey
        {
            get { return ConfigurationManager.AppSettings["haoservice_templatekey"].ToString(); }
        }
        #endregion

        #region Winic
        public static string GetPostData_winic(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            return $"id={HaoService_Uid}&pwd=[REDACTED]";
        }
        public static string WinicAPI
        {
            get { return ConfigurationManager.AppSettings["winic_url"].ToString(); }
        }

        public static string HaoService_Uid
        {
            get { return ConfigurationManager.AppSettings["winic_uid"].ToString(); }
        }

        public static string HaoService_Pwd
        {
            get { return ConfigurationManager.AppSettings["winic_pwd"].ToString(); }
        }
        #endregion

        #region nuage
        public static string GetPostData_nuage(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
                {
                    return string.Empty;
                }

                JObject json = JObject.Parse(venderInfo.SMSInfo);
                var telCode = "86";  //大陸國際碼
                tel = telCode + tel;

                var signature = json["signature"] != null ? json["signature"].ToString() : string.Empty;    //簽名需為公司名稱或產品名稱; 測試時, 使用DIGI
                var validcode = $"【{signature}】您好，您的验证码是{vCode}。";                              //sms content; 【DIGI】您好，您的验证码是1234。86132907128720783d97981d9
                var name = json["name"] != null ? json["name"].ToString() : string.Empty;                   //phone number ( digital only )
                var pwd = [REDACTED]"pwd"] != null ? json["pwd"].ToString() : string.Empty;                      //api user password
                var apiKey = [REDACTED]"apikey"] != null ? json["apikey"].ToString() : string.Empty;             //apiKey

                var checksum = MD5Service.Hash(validcode + tel + apiKey).ToLower();

                Dictionary<string, string> dic = new Dictionary<string, string>();

                dic["validcode"] = validcode;
                dic["receiver"] = tel;
                dic["name"] = name;
                dic["pwd"] = pwd;
                dic["checksum"] = checksum;
                var returnStr = string.Join("&", dic.Select(m => m.Key + "=" + m.Value).ToArray());
                return returnStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        public static string Nuage_Url
        {
            get { return ConfigurationManager.AppSettings["nuage_url"].ToString(); }
        }
        #endregion

        #region onecode
        public static string OneCodAPI
        {
            get { return ConfigurationManager.AppSettings["onecode_url"].ToString(); }
        }

        public static string OneCode_Method
        {
            get { return ConfigurationManager.AppSettings["onecode_method"].ToString(); }
        }

        public static string GetPostData_OneCode(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
                {
                    return string.Empty;
                }
                JObject json = JObject.Parse(venderInfo.SMSInfo);
                var telCode = "86";  //大陸國際碼
                tel = telCode + tel;
                //sms content; 【DDQP】您好，您的验证码是1234。
                var validcode = $"【LDR】您好，您的验证码是 {vCode}";

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["email"] = json["email"] != null ? json["email"].ToString() : string.Empty;
                dic["secret"] = json["secret"] != null ? json["secret"].ToString() : string.Empty;
                dic["method"] = "1";
                dic["sms_msg"] = HttpUtility.UrlEncode(validcode);
                dic["phone"] = tel;
                var returnStr = string.Join("&", dic.Select(m => m.Key + "=" + m.Value).ToArray());
                return returnStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
        #endregion

        #region SMSGo
        public static string SmsGoAPI
        {
            get { return ConfigurationManager.AppSettings["smsgo_url"].ToString(); }
        }
        public static string SmsGoBackAPI
        {
            get { return ConfigurationManager.AppSettings["smsgo_back_url"].ToString(); }
        }

        public static string GetPostData_SmsGo(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
                {
                    return string.Empty;
                }
                JObject json = JObject.Parse(venderInfo.SMSInfo);
                var telCode = "%2b86";  //大陸國際碼
                tel = telCode + tel;
                //sms content; 【DDQP】您好，您的验证码是1234。
                //var validcode = $"Your code is {vCode}";
                var validcode = $"您好，您的验证码是{vCode}";

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["username"] = json["username"] != null ? json["username"].ToString() : string.Empty;  //會員帳號
                dic["password"] = json["apikey"] != null ? json["apikey"].ToString() : string.Empty;  //會員密碼 或 API Key
                /* 接收簡訊之手機號碼，一次發送多筆號碼可用逗號隔開(不可超過 50 筆)。
                 * 國內號碼 09 開頭，十碼的數字；
                 * 國際號碼請在開頭多個 % 2b 例如傳大陸: % 2b8613681912700
                 * 其中 86 是大陸國碼，後面(以 13、15、18 開頭) 11 位數字是大陸手機號碼。*/
                dic["dstaddr"] = tel;
                /* BIG5/ ASCII / UCS2 / PBIG5 / PASCII / LBIG5 / LASCII / LUCS2 / PUSH
                 * 預設值為 BIG5(註: 此 encoding 可提供簡訊購作訊息處理，以及手機接收訊息後該用何
                 * 種編碼讀取等之用。P 表 POPUP 簡訊、L 表長簡訊、PUSH 表 wap push)
                 */
                dic["encoding"] = "UCS2";
                /* smbody 簡訊內容，中英文長度為 70 個字元，純英文為 160 個字元
                 * 若 encoding 為 LBIG5 / LASCII / LUCS2，則大小為 330 個中英文字
                 * 若 encoding 為 PUSH，則此欄為 wap push title
                 */
                dic["smbody"] = HttpUtility.UrlEncode(validcode);
                /* 預約時間，格式為 YYYY/ MM / DD hh24: mm: ss */
                //dic["dlvtime"] = tel;
                
                /* wapurl PUSH 當 encoding 設為 PUSH 才可以使用這個 tag*/
                //dic["wapurl"] = "";
                
                /* receiver 若有回覆簡訊時，vender 用來接收該回覆訊息的網址。(需另計點)
                 * 當 encoding 設為 BIG5 / ASCII / UCS2 才可以使用這個 tag*/
                //dic["replyurl"] = tel;

                /* 收取用戶回覆的天數，預設為 3(天)最大值不能超過 30(天)，
                 * 當 encoding 設為 BIG5/ ASCII / UCS2 才可以使用這個 tag */
                //dic["replydays"] = tel;
                
                /* 狀態回報網址，預設為空字串(不回報) */
                dic["response"] = "";
                /* 回傳訊息之格式: JSON / XML / 省略(default，為一般之文字內容) */
                dic["rtype"] = "JSON";

                var returnStr = string.Join("&", dic.Select(m => m.Key + "=" + m.Value).ToArray());
                return returnStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }


        #endregion

        #region ezlink
        public static string GetPostData_EzLink(string vCode, string tel, SMSVenderInfo venderInfo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(venderInfo.SMSInfo))
                {
                    return string.Empty;
                }
                JObject json = JObject.Parse(venderInfo.SMSInfo);
                var telCode = "0086";  //大陸國際碼 (如:+86 或 0086)
                tel = telCode + tel;
                //sms content; 【DDQP】您好，您的验证码是1234。
                var sign = json["sign"] != null ? json["sign"].ToString() : string.Empty;
                var validcode = $"【{sign}】您的验证码是{vCode}。如非本人操作，请忽略本简讯";

                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["id"] = json["id"] != null ? json["id"].ToString() : string.Empty;
                dic["to"] = tel;
                dic["msg"] = HttpUtility.UrlEncode(validcode);
                var returnStr = string.Join("&", dic.Select(m => m.Key + "=" + m.Value).ToArray());
                return returnStr;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
        }
        #endregion
        public static string Logname { get { return "sms.log"; } }

    }
}