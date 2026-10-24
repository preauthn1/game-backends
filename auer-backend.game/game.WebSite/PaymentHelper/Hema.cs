using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
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
    public class Hema
    {
        static string Hema_logFile { get { return "Hema.log"; } }

        public static PayState WithdrawCallback(string logGuid)
        {
            try
            {
                var requestType = HttpContext.Current.Request.RequestType;

                Stream stream = HttpContext.Current.Request.InputStream;
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                var responseString = Encoding.UTF8.GetString(bytes);

                SiteHelper.Log(logGuid, $"支付回调 [FormData {requestType}] {responseString}", Hema_logFile);


                JObject json = JObject.Parse(responseString);

                var sign = json["REP_HEAD"]["sign"] != null ? json["REP_HEAD"]["sign"].ToString() : string.Empty;                
                var orderState = json["REP_BODY"]["orderState"] != null ? json["REP_BODY"]["orderState"].ToString() : string.Empty;
                var tranSeqId = json["REP_BODY"]["tranSeqId"] != null ? json["REP_BODY"]["tranSeqId"].ToString() : string.Empty;
                var orderId = json["REP_BODY"]["orderId"] != null ? json["REP_BODY"]["orderId"].ToString() : string.Empty;
                var payTime = json["REP_BODY"]["payTime"] != null ? json["REP_BODY"]["payTime"].ToString() : string.Empty;
                var orderAmt = json["REP_BODY"]["orderAmt"] != null ? json["REP_BODY"]["orderAmt"].ToString() : string.Empty;


                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("sign", sign);
                parameters.Add("orderState", orderState);
                parameters.Add("tranSeqId", tranSeqId);
                parameters.Add("orderId", orderId);
                parameters.Add("payTime", payTime);
                parameters.Add("orderAmt", orderAmt);

                //if (CheckSign(logGuid, parameters))
                //{    
                    var res = Service.WithdrawCallbackByHema(orderId, orderState, tranSeqId, string.Empty);
                    if (res.Result)
                    {
                        SiteHelper.Log(logGuid, $"提现回调成功 orderId={orderId}&orderState={orderState}&tranSeqId={tranSeqId}&orderAmt={orderAmt}", Hema_logFile);
                        return PayState.Success;
                    }
                    else
                    {
                        SiteHelper.Log(logGuid, $"提现回调错误 {res.Message}", Hema_logFile);
                        return PayState.Error;
                    }
                //}
                //else
                //{
                //    SiteHelper.Log(logGuid, string.Format("提现异步回调非法签名"), Hema_logFile);
                //    return PayState.SignError;
                //}
            }
            catch (Exception ex)
            {
                SiteHelper.Log(logGuid, string.Format("提现异步回调异常错误 {0}", ex.ToString()), Hema_logFile);
                return PayState.Error;
            }
        }

        private static bool CheckSign(string guid, Dictionary<string, string> paras)
        {
            var sign = paras["sign"].ToString();
            paras.Remove("sign");

            var sort = paras.OrderBy(k => k.Key);
            paras = sort.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

            string str = string.Join("&", paras.Select(m => string.Format("{0}={1}", m.Key, m.Value))) + $"&key={Hema_MD5KEY}";            
            string md5 = Md5(str, Encoding.UTF8).ToUpper();
            string rsa_key = RSAPrivateKeyJava2DotNet(Hema_priKey);
            string cSign = RSAEncrypt(rsa_key, md5);


            SiteHelper.Log(guid, string.Format("支付回调签名 cSign={0} sign={1} srt={2}", cSign, sign, str), Hema_logFile);
            return sign == cSign;
        }

        static string Md5(string str, Encoding ec)
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
                    //byte[] encrypt = rsa.Encrypt(temp, false);
                    byte[] encrypt = rsa.SignData(temp, CryptoConfig.MapNameToOID("SHA256"));

                    ouput.Write(encrypt, 0, encrypt.Length);
                }
                return Convert.ToBase64String(ouput.ToArray());
            }
        }
    }
}