using game.Entity.Pay;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using static game.WebSite.PayConfig;
using example.invalid;

namespace game.WebSite.PaymentHelper
{
    public class BeifuPay
    {
        
        /// <summary>
        /// 创建订单(跳转网页链接)
        /// </summary>
        /// <param name="order"></param>
        /// <param name="guid"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            var p_syspwd = [REDACTED];

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "p_name", BeifuPay_uname },
                { "p_type", tradeCode },
                { "p_oid", order.Id.ToString()},
                { "p_money", order.Amount.ToString("f2")},                
                { "p_url", BeifuPay_NotifyUrl },
                { "p_surl", BeifuPay_sUrl},
                { "p_remarks", order.UserID.ToString() },
                { "p_syspwd", p_syspwd }
            };

            string param = string.Join("!", paras.Select((kvp) => kvp.Key + "=" + kvp.Value.ToString()));
            var encoded_params = DESEncrypt(HttpUtility.UrlEncode(param), BeifuPay_DesKey);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data] params:{param}, uname:{BeifuPay_uname} ", BeifuPay_LogFile);

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "params", encoded_params },
                { "uname", BeifuPay_uname }
            };         

            var url = $"<html><body><form method='post' name='form' action='{BeifuPay_Url}' enctype=\"application/x-www-form-urlencoded\">" +
                    string.Join("", data.Select(m => string.Format("<input type='hidden' name='{0}' value='{1}' >", m.Key, m.Value))) +
                    "</form><script>window.onload = function(){document.forms['form'].submit();}</script></body></html>";

            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({url}) ", BeifuPay_LogFile);

            var api = Service.CreateRedirectData((int)PayGatewayType.BeifuPay, url);

            if (api.Result)
                return api.Extend.ToString();
            else
                return string.Empty;

        }


        /// <summary>
        /// 支付成功回调
        /// </summary>
        /// <param name="logGuid"></param>
        /// <returns></returns>
        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("贝富支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), BeifuPay_LogFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("贝富支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), BeifuPay_LogFile);
                }
                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("贝富支付回调非法签名"), BeifuPay_LogFile);
                    return PayState.SignError;
                }

                var p_name = parameters.ContainsKey("p_name") ? parameters["p_name"] : string.Empty;                  //商户号
                var p_oid = parameters.ContainsKey("p_oid") ? parameters["p_oid"] : string.Empty;                     //订单号
                var p_money = parameters.ContainsKey("p_money") ? parameters["p_money"] : string.Empty;               //订单交易额 
                var p_code = parameters.ContainsKey("p_code") ? parameters["p_code"] : string.Empty;                  //结果标识(1:成功)
                var p_remarks = parameters.ContainsKey("p_remarks") ? parameters["p_remarks"] : string.Empty;         //备注(原样返回信息)
                var p_sid = parameters.ContainsKey("p_sid") ? parameters["p_sid"] : string.Empty;                     //系统订单号（平台系统生成的订单号）
                var p_syspwd = [REDACTED]"p_syspwd") ? parameters["p_syspwd"] : string.Empty;            //系统密码验证
                var p_md5 = parameters.ContainsKey("p_md5") ? parameters["p_md5"] : string.Empty;                     //Md5 加密验证

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(p_oid));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 贝富支付回调 訂單狀態已經完成,無須金幣充值", order.Id), BeifuPay_LogFile);
                    return PayState.Success;
                }

                //進行金幣充值
                string msg = "";

                if (p_code.Equals("1") && (Convert.ToDecimal(p_money) == order.Amount))
                {
                    var data = $"贝富支付回调 完成支付: 支付订单号: {p_oid} , 渠道订单号: {p_sid}, 状态: success";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, p_sid, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 贝富支付回调 金幣充值成功 支付订单号:{p_oid} 金额:{order.Amount} 渠道订单号:{p_sid}", BeifuPay_LogFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 贝富支付回调 金幣充值错误 {msg}", BeifuPay_LogFile);

                        Service.CDFailOrder(order.Id, p_sid, $"贝富支付订单号:{p_oid} , 渠道订单号: {p_sid} , 支付金额: {p_money}, 状态: fail");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, $"{order.Id}  贝富支付回调 支付無效 支付订单号:{p_oid}, 渠道订单号:{p_sid}, 支付金额:{p_money}, 状态: fail", BeifuPay_LogFile);
                Service.CDFailOrder(order.Id, "0", $"贝富支付订单号:{p_oid} , 渠道订单号: {p_sid} , 支付金额: {p_money}, 状态: fail");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("贝富支付回调 异常错误 {0}", ex.ToString()), BeifuPay_LogFile);
                return PayState.Error;
            }
        }


        private static bool CheckSign(string guid, out Dictionary<string, string> parameters)
        {
            var requestType = HttpContext.Current.Request.RequestType;
            if (requestType == "GET")
            {
                parameters = HttpContext.Current.Request.QueryString.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                parameters = HttpContext.Current.Request.Form.AllKeys.Where(x => !string.IsNullOrEmpty(x)).Cast<string>()
                    .ToDictionary(k => k, v => HttpContext.Current.Request[v]).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            var p_name = parameters.ContainsKey("p_name") ? parameters["p_name"] : string.Empty;                  //商户号
            var p_oid = parameters.ContainsKey("p_oid") ? parameters["p_oid"] : string.Empty;                     //订单号
            var p_money = parameters.ContainsKey("p_money") ? parameters["p_money"] : string.Empty;               //订单交易额 
            var p_code = parameters.ContainsKey("p_code") ? parameters["p_code"] : string.Empty;                  //结果标识(1:成功)
            var p_remarks = parameters.ContainsKey("p_remarks") ? parameters["p_remarks"] : string.Empty;         //备注(原样返回信息)
            var p_sid = parameters.ContainsKey("p_sid") ? parameters["p_sid"] : string.Empty;                     //系统订单号（平台系统生成的订单号）
            var p_syspwd = [REDACTED]"p_syspwd") ? parameters["p_syspwd"] : string.Empty;            //系统密码验证
            var p_md5 = parameters.ContainsKey("p_md5") ? parameters["p_md5"] : string.Empty;                     //Md5 加密验证

          
            var param = p_name + p_oid + p_money + BeifuPay_SysPwd;
            var cSign = Md5(param + BeifuPay_MD5Key);
            SiteHelper.Log(guid, $"贝富支付支付回调签名 data={param} cSign={cSign} sign={p_md5}", BeifuPay_LogFile);
            return p_md5 == cSign;
        }

        /// <summary>
        /// DES加密算法
        /// </summary>
        /// <param name="encryptString">要加密的字符串</param>
        /// <param name="sKey">加密码Key</param>
        /// <returns>正确返回加密后的结果，错误返回源字符串</returns>
        private static string DESEncrypt(string encryptString, string sKey)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(sKey);
                byte[] keyIV = keyBytes;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);

                DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();

                // java 默认的是ECB模式，PKCS5padding；c#默认的CBC模式，PKCS7padding 所以这里我们默认使用ECB方式
                desProvider.Padding = PaddingMode.PKCS7;
                desProvider.Mode = CipherMode.CBC;
                MemoryStream memStream = new MemoryStream();
                CryptoStream crypStream = new CryptoStream(memStream, desProvider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write);

                crypStream.Write(inputByteArray, 0, inputByteArray.Length);
                crypStream.FlushFinalBlock();
                return Convert.ToBase64String(memStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
        }


        /// <summary>
        /// MD5签名
        /// </summary>
        /// <param name="QueryStr"></param>
        /// <returns></returns>
        private static string Md5(string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            bytes = md5.ComputeHash(bytes);
            md5.Clear();
            string ret = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                ret += Convert.ToString(bytes[i], 16).PadLeft(2, '0');
            }
            return ret.PadLeft(32, '0').ToLower();
        }
    }
}
