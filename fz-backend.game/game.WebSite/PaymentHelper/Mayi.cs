using game.Entity.Money;
using game.Entity.Pay;
using game.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class Mayi
    {
        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), Mayi_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), Mayi_logFile);
                }

                Dictionary<string, string> parameters = null;
                if (!CheckSign(logGuid, ref parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), Mayi_logFile);
                    return PayState.SignError;
                }                

                var orderId = parameters.ContainsKey("api_sn") ? parameters["api_sn"] : string.Empty;
                var orderAmt = parameters.ContainsKey("money") ? parameters["money"] : string.Empty;
                var orderStatus = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                var transId = parameters.ContainsKey("order_sn") ? parameters["order_sn"] : string.Empty;
                var transData = parameters.ContainsKey("feedback_error") ? parameters["feedback_error"] : string.Empty;
                var status = ConvertLogStatus.Queueing;

                //30:执行异常,40:失败结束,50:已完成
                if (orderStatus == "50") status = ConvertLogStatus.AlreadyPaid;
                else status = ConvertLogStatus.AutoPaymentFailed;

                var res = Service.WithdrawCallback(orderId, (int)status, transId, transData);
                if (res.Result)
                {
                    SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={status}&tranSeqId={transId}&orderAmt={orderAmt}", Mayi_logFile);
                    return PayState.Success;
                }
                else
                {
                    SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Mayi_logFile);
                    return PayState.Error;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Mayi_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, ref Dictionary<string, string> parameters)
        {           
            var data = HttpContext.Current.Request["params"];

            if (string.IsNullOrEmpty(data))
                return false;

            string key = Mayi_Key.Substring(0, 24);           
            string iv = Mayi_Key.Substring(0, 8);

            
            string descodeData = Encoding.UTF8.GetString(Des3DecodeCBC(Encoding.UTF8.GetBytes(key), 
                                                                       Encoding.UTF8.GetBytes(iv),
                                                                       Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(data)))));
                     

            parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(descodeData);

            var sign = parameters["sign"];
            parameters.Remove("sign");
          
            var sort = parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);          

            
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={Mayi_Key}";
            var cSign = Md5(str, Encoding.UTF8).ToUpper();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Mayi_logFile);

            return sign == cSign;
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        #region CBC模式**  
     
        /// <summary>  
        /// DES3 CBC模式解密  
        /// </summary>  
        /// <param name="key">密钥</param>  
        /// <param name="iv">IV</param>  
        /// <param name="data">密文的byte数组</param>  
        /// <returns>明文的byte数组</returns>  
        public static byte[] Des3DecodeCBC(byte[] key, byte[] iv, byte[] data)
        {
            try
            {                
                MemoryStream msDecrypt = new MemoryStream(data);
                TripleDESCryptoServiceProvider tdsp = new TripleDESCryptoServiceProvider();
                tdsp.Mode = CipherMode.CBC;
                tdsp.Padding = PaddingMode.PKCS7;
              
                CryptoStream csDecrypt = new CryptoStream(msDecrypt,
                    tdsp.CreateDecryptor(key, iv),
                    CryptoStreamMode.Read);
             
                byte[] fromEncrypt = new byte[data.Length];
              
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
                return fromEncrypt;
            }
            catch (CryptographicException e)
            {
                throw e;              
            }
        }
        #endregion
    }
}