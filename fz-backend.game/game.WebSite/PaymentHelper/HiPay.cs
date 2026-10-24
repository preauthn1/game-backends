using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using game.Entity.Pay;
using game.Infrastructure;
using static game.WebSite.PayConfig;
using Newtonsoft.Json.Linq;
using example.invalid;
using Newtonsoft.Json;

namespace game.WebSite.PaymentHelper
{
    public class HiPay
    {
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;            

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "account", order.UserID.ToString() },     //平台会员账号
                { "amount", order.Amount.ToString() },      //充值金额
                { "key", HiPay_Key },                       //商户密钥
                { "merchantOrderNo", order.Id.ToString() }, //商户订单号                
                { "merchantNo", HiPay_merchantNo },         //商户号
                { "notifyUrl", HiPay_CallbackUrl },         //回调接口地址
                { "type", tradeCode },                      //支付类型
            };                        

            var data = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}")));

            var public_key = PayConfig.Wfu_RSAPublicKeyJava2DotNet(HiPay_RSA_PublicKey);
            var encrtptData = RSAEncrypt(public_key, data);
            var pr = HttpUtility.UrlEncode(encrtptData, Encoding.UTF8);

            var paysign = string.Join("&", paras.Select((m => $"{m.Key}={m.Value}"))) + "8ab211a6536711e893d10242ac11as02";
            var sign = Md5(paysign);

        
            SiteHelper.Log(guid, $"{order.Id} 支付请求 ({HiPay_URL}) pr={data}&sign={paysign}", HiPay_logFile);
            SiteHelper.Log(guid, $"{order.Id} 支付请求 [data]({HiPay_URL}) pr={pr}&sign={sign}", HiPay_logFile);

            url = $"{ HiPay_URL}?pr={pr}&sign={sign}";

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;
                if (requestType == "GET")
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.QueryString.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.QueryString[key]))), HiPay_logFile);
                }
                else
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", string.Join("&", HttpContext.Current.Request.Form.AllKeys.Select(key => key + "=" + HttpContext.Current.Request.Form[key]))), HiPay_logFile);
                }


                Dictionary<string, string> parameters = null;

                if (!CheckSign(logGuid, out parameters))
                {
                    SiteHelper.Log(logGuid, string.Format("支付回调非法签名"), HiPay_logFile);
                    return PayState.SignError;
                }

                var platOrderNo = parameters.ContainsKey("platOrderNo") ? parameters["platOrderNo"] : string.Empty;
                var merchantOrderNo = parameters.ContainsKey("merchantOrderNo") ? parameters["merchantOrderNo"] : string.Empty;
                var amount = parameters.ContainsKey("amount") ? parameters["amount"] : string.Empty;
                var status = parameters.ContainsKey("status") ? parameters["status"] : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(merchantOrderNo));
                if (order.Status == CDOrderStatus.Paied)
                {                    
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 訂單狀態已經完成,無須金幣充值", HiPay_logFile);
                    return PayState.Success;
                }
                
                if (status == "0")
                {
                    SiteHelper.Log(logGuid, $"{order.Id} 支付回调 订单生成: {merchantOrderNo}", HiPay_logFile);
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if ((status == "1") && (Convert.ToDecimal(amount) == order.Amount))
                {
                    var data = $"支付回调 完成支付: 支付订单号: {merchantOrderNo}, 状态: {status}";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, platOrderNo, data, ref msg) == true)
                    {                        
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 支付订单号:{platOrderNo} 金额:amount={order.Amount}", HiPay_logFile);
                        return PayState.Success;
                    }
                    else
                    {                        
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", HiPay_logFile);
                        Service.CDFailOrder(order.Id, platOrderNo, $"金额: {amount}, 状态: {status}");
                        return PayState.Error;
                    }
                }
                
                SiteHelper.Log(logGuid, $"{order.Id} 支付回调 支付無效 支付订单号:{platOrderNo}, 金额:{amount}, 状态:{status}", HiPay_logFile);
                Service.CDFailOrder(order.Id, platOrderNo, $"支付订单号:{platOrderNo}, 金额 {amount}, 状态: {status}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), HiPay_logFile);
                return PayState.Error;
            }
        }

        private static string RSAEncrypt(string publicKey, string content)
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(publicKey);

            int bufferSize = (rsa.KeySize / 8) - 11;
            byte[] buffer = new byte[bufferSize];
            //分段加密
            using (MemoryStream input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (MemoryStream ouput = new MemoryStream())
            {
                while (true)
                {
                    int readLine = input.Read(buffer, 0, bufferSize);
                    if (readLine <= 0)
                    {
                        break;
                    }
                    byte[] temp = new byte[readLine];
                    Array.Copy(buffer, 0, temp, 0, readLine);
                    byte[] encrypt = rsa.Encrypt(temp, false);
                    ouput.Write(encrypt, 0, encrypt.Length);
                }
                return Convert.ToBase64String(ouput.ToArray());
            }
        }

        private static string RSADecrypt(string privateKey, string encryptedContent)
        {
            RSACryptoServiceProvider rsa = CreateRsaProviderFromPrivateKey(privateKey);

            var decryptString = Encoding.UTF8.GetString(rsa.Decrypt(Convert.FromBase64String(encryptedContent), false));

            return decryptString;
        }

        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider(2048);
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        /// <summary>
        /// 与java的MD5加密保持一致
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Md5(string str)
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

            var sign = parameters["sign"].ToLower();
            parameters.Remove("sign");
            parameters = parameters.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

            var str = string.Join("&", parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select(m => string.Format("{0}={1}", m.Key, m.Value))) + "&key=" + HiPay_Key;
            var cSign = Md5(str).ToLower();
            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), HiPay_logFile);

            return sign == cSign;
        }
    }
}