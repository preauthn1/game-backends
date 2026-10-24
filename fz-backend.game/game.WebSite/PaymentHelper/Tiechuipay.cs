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
using System.Security.Cryptography.X509Certificates;

namespace game.WebSite.PaymentHelper
{
    public class Tiechuipay
    {   
        public static string post(CDPayOrder order, string guid, string tradeCode, ref string message)
        {
            string url = string.Empty;

            var secKey = Guid.NewGuid().ToString("N").Substring(0,16);


            Dictionary<string, string> paras1 = new Dictionary<string, string>
            {
                { "agentCode", Tiechuipay_agentCode },      //代理商编号
                { "merchantCode", Tiechuipay_merchantCode },//商户号
                { "downOrderNum", order.Id.ToString() },    //订单号
                { "totalAmount", order.Amount.ToString() }, //订单金额
                { "channelCode", Tiechuipay_channelCode },  //渠道编号
                { "interfaceType", tradeCode.Split(',').Last() },//接口类型
                { "uniqueUserId", order.UserID.ToString() },//唯一识别号
                { "payType", tradeCode.Split(',').First() },//支付方式
                { "goodsName", "充值" },                    //商品名称
                { "ip", order.IPAddress },                  //用户Ip
                { "describe", "充值" },                     //商品描述
                { "callBackUrl", Tiechuipay_CallbackUrl },  //异步通知
                { "successUrl", "0.0.0.0" }               //前台回调
            };
            
            var dataJson = JsonConvert.SerializeObject(paras1);
            var encryptData = AESEncrypt(dataJson, secKey);
            var public_key = PayConfig.Wfu_RSAPublicKeyJava2DotNet(Tiechuipay_Key);

            var encrtptKey = RSAEncrypt(public_key, secKey);
            var signData = RSAEncrypt(public_key, dataJson);

            Dictionary<string, string> paras = new Dictionary<string, string>
            {
                { "merchantCode", Tiechuipay_merchantCode },//商户号
                { "agentCode", Tiechuipay_agentCode },      //代理商编号
                { "encryptData", encryptData },             //加密后的请求报文
                { "encrtptKey", encrtptKey },               //加密后的AES
                { "signData", signData },                   //签名
                { "version", "3.0.0" }                      //版本
            };

            var data = JsonConvert.SerializeObject(paras);
            SiteHelper.Log(guid, string.Format("{0} 支付请求 [data]({1}) {2}", order.Id, Tiechuipay_URL, data), Tiechuipay_logFile);

            //呼叫支付平台
            var responseString = SiteHelper.HttpPost(Tiechuipay_URL, data, "application/json");
            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [response] {1}", order.Id, responseString), Tiechuipay_logFile);

            JObject json = JObject.Parse(responseString);
            var result_signData = json["signData"] != null ? json["signData"].ToString() : string.Empty;
            var result_encrtptKey = json["encrtptKey"] != null ? json["encrtptKey"].ToString() : string.Empty;
            var result_encryptData = json["encryptData"] != null ? json["encryptData"].ToString() : string.Empty;
            
          
            var decrypt_aesKey = RSADecrypt(Tiechuipay_RSA_PrivateKey, result_encrtptKey);
            var decrypt_json = AESDecrypt(result_encryptData, decrypt_aesKey);
            var decrypt_sign = RSAEncrypt(PayConfig.Wfu_RSAPublicKeyJava2DotNet(Tiechuipay_RSA_PublicKey), decrypt_json);

            SiteHelper.Log(guid, string.Format("{0} 支付请求结果 [Decrypt] {1}", order.Id, decrypt_json), Tiechuipay_logFile);
            json = JObject.Parse(decrypt_json);

            var status = json["code"] != null ? json["code"].ToString() : string.Empty;

            if (!status.Equals("20000"))
            {
                var msg = json["msg"] != null ? json["msg"].ToString() : string.Empty;
                message = string.Format("{0} 支付请求失敗 {1} {2}", order.Id, status, msg);

                //更新狀態
                Service.CDFailOrder(order.Id, string.Empty, $"{status} {msg}");
                SiteHelper.Log(guid, message, CfanPay_logFile);
            }
            else
            {   
                Service.CDModifyTransID(order.Id, "0");
                url = json["data"]["payUrl"] != null ? json["data"]["payUrl"].ToString() : string.Empty;
            }

            return url;
        }

        public static PayState call(string logGuid)
        {
            try
            {
                Stream stream = HttpContext.Current.Request.InputStream;
                var b = new byte[stream.Length];
                stream.Read(b, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(b);
                SiteHelper.Log(logGuid, string.Format("支付回调 [FormData] {0}", responseString), Tiechuipay_logFile);
                               
                JObject json = JObject.Parse(responseString);

                var result_signData = json["signData"] != null ? json["signData"].ToString() : string.Empty;
                var result_encrtptKey = json["encrtptKey"] != null ? json["encrtptKey"].ToString() : string.Empty;
                var result_encryptData = json["encryptData"] != null ? json["encryptData"].ToString() : string.Empty;

                var decrypt_aesKey = RSADecrypt(Tiechuipay_RSA_PrivateKey, result_encrtptKey);
                var decrypt_json = AESDecrypt(result_encryptData, decrypt_aesKey);
                SiteHelper.Log(logGuid, string.Format("支付回调 [Decrypt] {0}", decrypt_json), Tiechuipay_logFile);

                json = JObject.Parse(decrypt_json);

                var agentCode = json["agentCode"] != null ? json["agentCode"].ToString() : string.Empty;
                var buyerPayAmount = json["buyerPayAmount"] != null ? json["buyerPayAmount"].ToString() : string.Empty;
                var orderNo = json["downOrderNum"] != null ? json["downOrderNum"].ToString() : string.Empty;
                var flowNumber = json["flowNumber"] != null ? json["flowNumber"].ToString() : string.Empty;
                var merchantCode = json["merchantCode"] != null ? json["merchantCode"].ToString() : string.Empty;
                var orderNum = json["orderNum"] != null ? json["orderNum"].ToString() : string.Empty;
                var responseCode = json["responseCode"] != null ? json["responseCode"].ToString() : string.Empty;
                var responseMsg = json["responseMsg"] != null ? json["responseMsg"].ToString() : string.Empty;
                var time = json["time"] != null ? json["time"].ToString() : string.Empty;
                var totalAmount = json["totalAmount"] != null ? json["totalAmount"].ToString() : string.Empty;


                //檢查訂單狀態
                var order = Service.CDGetPayOrderByID(Convert.ToInt32(orderNo));
                if (order.Status == CDOrderStatus.Paied)
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 訂單狀態已經完成,無須金幣充值", order.Id), Tiechuipay_logFile);
                    return PayState.Success;
                }

                if (responseCode != "20000")
                {
                    SiteHelper.Log(logGuid, string.Format("{0} 支付回调 订单失敗 responseMsg:{1}", order.Id, responseMsg), Tiechuipay_logFile);
                    Service.CDFailOrder(order.Id, orderNum, $"{responseCode} {responseMsg}");
                    return PayState.Error;
                }

                //進行金幣充值
                string msg = "";

                if (Convert.ToDouble(totalAmount) == order.Amount)
                {
                    var data = $"完成支付";

                    if (Service.CDFinishOrder(order.Id, order.Amount, SystemHelper.UserIPAddress, orderNum, data, ref msg) == true)
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值成功 orderNum,:{orderNum},totalAmount:{totalAmount}", Tiechuipay_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"{order.Id} 支付回调 金幣充值错误 {msg}", Tiechuipay_logFile);
                        Service.CDFailOrder(order.Id, orderNum, $"{msg}");
                        return PayState.Error;
                    }
                }

                SiteHelper.Log(logGuid, string.Format($"{order.Id}  支付回调 支付無效 orderNum:{orderNum},totalAmount:{totalAmount}"), Tiechuipay_logFile);
                Service.CDFailOrder(order.Id, orderNum, $"responseCode:{responseCode},responseMsg:{responseMsg}");
                return PayState.Error;
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("支付回调 异常错误 {0}", ex.ToString()), Tiechuipay_logFile);
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
        /// AES加密算法  
        /// </summary>  
        public static string AESEncrypt(string text, string key)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(text);
            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            
            var transform = aes.CreateEncryptor();
            return Convert.ToBase64String(transform.TransformFinalBlock(sourceBytes, 0, sourceBytes.Length));
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        public static string AESDecrypt(string text, string key)
        {
            var encryptBytes = Convert.FromBase64String(text);
            var aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
           
            var transform = aes.CreateDecryptor();
            return Encoding.UTF8.GetString(transform.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));
        }
    }
}