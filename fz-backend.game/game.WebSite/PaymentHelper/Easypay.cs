using game.Entity.Pay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static game.WebSite.PayConfig;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using game.Infrastructure;
using Newtonsoft.Json.Linq;
using example.invalid;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace game.WebSite.PaymentHelper
{
    public class Easypay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "amount", order.Amount.ToString("F2") },
                { "authId", Easypay_AuthId },
                { "type", tradeCode },             
                { "outTradeNo", order.Id.ToString() },
                { "nonceStr", Guid.NewGuid().ToString("N").Substring(0, 10) },
                { "notifyURL", Easypay_NotifyUrl },
                { "successToURL", Easypay_CallbackUrl }
            };

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            var signdata = string.Join("&", paras.Where(x=>!string.IsNullOrEmpty(x.Value)).OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            string rsa_key = RSAPrivateKeyJava2DotNet(Easypay_RSA_PrivateKey);
            string pay_rsasign = RSAEncrypt(rsa_key, signdata);
          
            paras.Add("sign", pay_rsasign);

            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Easypay_Url, data), Easypay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPostJson(Easypay_Url, data);
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Easypay_logFile);
             
            JObject json = JObject.Parse(responseString);
            var retCode = json["retCode"] != null ? json["retCode"].ToString() : string.Empty;

            if (!retCode.Equals("1001000"))
            {
                var respmsg = json["retMsg"] != null ? json["retMsg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1}", order.Id, respmsg);

                //更新狀態
                //Service.CDFailOrder(order.Id, string.Empty, respmsg);
                SiteHelper.Log(guid, message, Easypay_logFile);
            }
            else
            {
                url = json["retData"]["payURL"] != null ? json["retData"]["payURL"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;  
                Stream stream = HttpContext.Current.Request.InputStream;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(bytes);

                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData {0}] {1}", requestType, responseString), Easypay_logFile);
                SiteHelper.Log(logGuid, string.Format("支付回调 [Headers] {0}", string.Join("&", HttpContext.Current.Request.Headers.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Headers.Get(key)))), Easypay_logFile);
                

                Dictionary<string, string> parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                if (HttpContext.Current.Request.Headers.Get("X-Auth-Sign") != null)                    
                {
                    parameters.Add("sign", HttpContext.Current.Request.Headers.Get("X-Auth-Sign"));

                    if (!CheckSign(logGuid, parameters))
                    {
                        SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), Easypay_logFile);
                        return PayState.SignError;
                    }
                }

                var orderNum = parameters.ContainsKey("outTradeNo") ? parameters["outTradeNo"] : string.Empty;
                var resultCode = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var transID = parameters.ContainsKey("transactionId") ? parameters["transactionId"] : string.Empty;

                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNum));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Easypay_logFile);
                    return PayState.Success;
                }


                //進行金幣充值
                string msg = "";

                if ((resultCode == "4" || resultCode == "9") && (Convert.ToDecimal(amount) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 状态: {resultCode}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, transID, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值成功 金额:{1}, 状态:{2}", order.Id, order.Amount, resultCode), Easypay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, string.Format("{0} 支付回调 金幣充值错误 {1}", order.Id, msg), Easypay_logFile);
                        Service.CDFailOrder(order.Id, transID, $"支付金额: {amount}, 状态: {resultCode}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format("{0}  支付回调 支付無效 支付金额(分):{1}, 状态:{2}", order.Id, amount, resultCode), Easypay_logFile);
                Service.CDFailOrder(order.Id, transID, $"支付金额 {amount}, 状态: {resultCode}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Easypay_logFile);
                return PayState.Error;
            }
        }

        private static string Md5(string str, Encoding ec)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return BitConverter.ToString(md5.ComputeHash(ec.GetBytes(str))).Replace("-", "");
        }

        static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        static string RSAEncrypt(string publicKey, string content)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            rsa.FromXmlString(publicKey);

            byte[] encrypt = rsa.SignData(Encoding.UTF8.GetBytes(content), new SHA256CryptoServiceProvider());                        
            return Convert.ToBase64String(encrypt.ToArray());            
        }       

        private static bool CheckSign(string guid, Dictionary<string, string> parameters)
        {
            var sign = parameters["sign"];
            parameters.Remove("sign");

            var sort = parameters.OrderBy(k => k.Key);
            parameters = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            var str = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).OrderBy(x => x.Key).Select(m => string.Format("{0}={1}", m.Key, m.Value)));

            string rsa_key = RSAPrivateKeyJava2DotNet(Easypay_RSA_PrivateKey);
            string cSign = RSAEncrypt(rsa_key, str);        

            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Easypay_logFile);

            return sign == cSign;
        }
    }
}