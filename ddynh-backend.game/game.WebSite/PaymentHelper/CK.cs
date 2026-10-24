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
    public class CK
    {
        static string CK_logFile { get { return "CK.log"; } }

        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {              
                Dictionary<string, string> parameters = null;

                if (CheckSign(logGuid, out parameters))
                {
                    var code = parameters["code"].ToString();
                    var msg = parameters["msg"].ToString();
                    var id = parameters["data[id]"].ToString();
                    var order_num = Convert.ToInt32(parameters["data[order_num]"]);
                    var state = Convert.ToInt32(parameters["data[state]"]);

                    var res = Service.WithdrawCallbackByCK(order_num, state, id, msg);
                    if (res.Result)
                    {
                        SiteHelper.Log(logGuid, $"提现回调成功 order_num={order_num}&state={state}&id={id}&msg={msg}", CK_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", CK_logFile);
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), CK_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), CK_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            var requestType = HttpContext.Current.Request.RequestType;         

            if (requestType == "GET")
            {
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Cast<string>()
             .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Cast<string>()
               .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            
            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras.Add("bank_card", parameters["data[bank_card]"].ToString());
            paras.Add("bank_name", parameters["data[bank_name]"].ToString());
            paras.Add("bank_type", parameters["data[bank_type]"].ToString());
            paras.Add("created_at", parameters["data[created_at]"].ToString());
            paras.Add("id", parameters["data[id]"].ToString());
            paras.Add("money", parameters["data[money]"].ToString());
            paras.Add("order_num", parameters["data[order_num]"].ToString());
            paras.Add("real_money", parameters["data[real_money]"].ToString());
            paras.Add("state", parameters["data[state]"].ToString());
           
            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            string sha1 = SHA1(str, Encoding.UTF8);
            string cSign = Md5(sha1, Encoding.UTF8).ToUpper(); 
            var sign = parameters["data[signature]"].ToString();

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), CK_logFile);
            return sign == cSign;
        }

        static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        static string SHA1(string str, Encoding ec)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] source = ec.GetBytes(str);
            byte[] crypto = sha1.ComputeHash(source);
            return Convert.ToBase64String(crypto);
        }
    }
}