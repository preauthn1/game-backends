using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;

namespace game.WebSite.PaymentHelper
{
    public class MCPay
    {
        private class ComparerString : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.CompareOrdinal(x, y);
            }
        }

        public static string post(CDPayOrder order, string guid, string tradeCode)
        {
            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "banktype", tradeCode },
                { "usernumber", order.Id.ToString() },
                { "paymoney", order.Amount.ToString() },
                { "callbackurl", MCPay_CallbackUrl }
            };

            //1.将p参数值进行参数名ASCII码从小到大排序（字典序）并拼接生成字符串p
            var par = GetParamSrc(data);
            //2.将拼接好的参数进行Base64的加密
            par = Base64Encode(par);
            //3.将Base64加密后的字符串par进行MD5加密生成sign
            var paysign = MD532(par);
            //4.将3个参数的值进行URL的UTF-8进行编码
            var appidurl = HttpUtility.UrlEncode(MCPay_Key, Encoding.UTF8);
            var parurl = HttpUtility.UrlEncode(par, Encoding.UTF8);
            var signurl = HttpUtility.UrlEncode(paysign, Encoding.UTF8);
            var url = MCPay_PayURL + "a=" + appidurl + "&p=" + parurl + "&s=" + signurl;
            SiteHelper.Log(guid, $"充值请求: {url}", MCPay_logFile);
            return url;
        }


        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), MCPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), MCPay_logFile);
                }

                if (CheckSign(logGuid))
                {
                    var playerId = HttpContext.Current.Request["id"];
                    var orderId = HttpContext.Current.Request["usernumber"];
                    var orderAmount = HttpContext.Current.Request["paymoney"];
                    var status = HttpContext.Current.Request["orderstatus"];
                    var sign = HttpContext.Current.Request["sign"];
                    var banktype = HttpContext.Current.Request["banktype"];

                    //檢查訂單狀態
                    var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderId));
                    if (order.Status == CDOrderStatus.Paied)
                    {
                        SiteHelper.Log(logGuid, string.Format("訂單狀態已經完成,無須金幣充值"), MCPay_logFile);
                        return PayState.Success;
                    }

                    //支付失败
                    if (status != "1")
                    {
                        SiteHelper.Log(logGuid, string.Format("支付失败"), MCPay_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付失败 playerId: {playerId} , MCPay訂單: {orderId} , orderAmount: {orderAmount},status: {status}");
                        return PayState.Error;
                    }

                    //進行金幣充值
                    string msg = "";

                    if (status == "1" && order.Amount == Convert.ToInt64(double.Parse(orderAmount)))
                    {
                        var data = $"完成支付: playerId: {playerId} , MCPay訂單: {orderId}, orderAmount: {orderAmount}, status: {status}";

                        if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, "0", data, ref msg) == true)
                        {
                            SiteHelper.Log(logGuid, $"金幣充值成功 orderid={order.Id} amount={order.Amount}", MCPay_logFile);
                            return PayState.Success;
                        }
                        else
                        {
                            SiteHelper.Log(logGuid, string.Format("金幣充值错误 {0}", msg), MCPay_logFile);
                            return PayState.Error;
                        }
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("支付無效"), MCPay_logFile);
                        Service.CDFailOrder(order.Id, orderId, $"支付無效 playerId:{playerId} , MCPay訂單: {orderId} , orderAmount: {orderAmount},status: {status}");
                        return PayState.Error;
                    }
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), MCPay_logFile);
                    return PayState.SignError;
                }
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调异常错误 {0}", ex.ToString()), MCPay_logFile);
                return PayState.Error;
            }
        }


        private static bool CheckSign(string guid)
        {

            var requestType = HttpContext.Current.Request.RequestType;
            Dictionary<string, string> parameters = null;

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

            parameters.Remove("s");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var sign = HttpContext.Current.Request["s"];
            var str = string.Join("&", parameters.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
            var cSign = MD532(str);
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), MCPay_logFile);

            return sign == cSign;
        }

        /// <summary>
        /// 参数按照参数名ASCII码从小到大排序（字典序）
        /// </summary>
        /// <param name="paramsMap"></param>
        /// <returns></returns>
        private static string GetParamSrc(Dictionary<string, string> paramsMap)
        {

            var vDic = paramsMap.OrderBy(x => x.Key, new ComparerString()).ToDictionary(x => x.Key, y => y.Value);

            StringBuilder str = new StringBuilder();

            foreach (KeyValuePair<string, string> kv in vDic)
            {
                string pkey = kv.Key;
                string pvalue = kv.Value;
                str.Append(pkey + "=" + pvalue + "&");
            }

            string result = str.ToString().Substring(0, str.ToString().Length - 1);

            return result;
        }


        /// <summary>
        /// Base64加密，采用utf8编码方式加密
        /// </summary>
        /// <param name="source">待加密的明文</param>
        /// <returns>加密后的字符串</returns>
        private static string Base64Encode(string source)
        {
            return Base64Encode(Encoding.UTF8, source);
        }

        /// <summary>
        /// Base64加密
        /// </summary>
        /// <param name="encodeType">加密采用的编码方式</param>
        /// <param name="source">待加密的明文</param>
        /// <returns></returns>
        private static string Base64Encode(Encoding encodeType, string source)
        {
            string encode = string.Empty;
            byte[] bytes = encodeType.GetBytes(source);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = source;
            }
            return encode;
        }

        /// <summary>
        /// Base64解密
        /// </summary>
        /// <param name="encodeType">解密采用的编码方式，注意和加密时采用的方式一致</param>
        /// <param name="result">待解密的密文</param>
        /// <returns>解密后的字符串</returns>
        private static string Base64Decode(Encoding encodeType, string result)
        {
            string decode = string.Empty;
            byte[] bytes = Convert.FromBase64String(result);
            try
            {
                decode = encodeType.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        /// <summary>
        /// 与java的MD5加密保持一致
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string MD532(string str)
        {
            var md5Csp = new MD5CryptoServiceProvider();
            byte[] md5Source = Encoding.UTF8.GetBytes(str);
            byte[] md5Out = md5Csp.ComputeHash(md5Source);
            string pwd = "";
            for (int i = 0; i < md5Out.Length; i++)
            {
                pwd += md5Out[i].ToString("x2");
            }
            return pwd;
        }
    }
}