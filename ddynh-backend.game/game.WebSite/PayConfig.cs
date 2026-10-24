using System;
using System.Configuration;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;
using System.Text;

namespace game.WebSite
{
    public class PayConfig
    {
        public static string productName { get { return "超级大玩家金币"; } }
        public static string defaultlog { get { return "paycenter.log"; } }

        public static string Payment_logFile { get { return "Payment.log"; } }

        public static string BotToken
        {
            get
            {
                return ConfigurationManager.AppSettings["botToken"].ToString();
            }
        }

        public static string[] Chatid
        {
            get { return ConfigurationManager.AppSettings["chatid"].Split(','); }
        }

        public static string SendAlertUrl
        {
            get { return ConfigurationManager.AppSettings["sendAlertUrl"]; }
        }

        public static string FinishOrder_logFile
        {
            get { return "FinishOrder.log"; }
        }

        public static string AESSecretKey
        {
            get { return ConfigurationManager.AppSettings["SecretKey"]; }
        }

        public static string PayCenterOpenRedirectUrl
        {
            get { return ConfigurationManager.AppSettings["PayCenterOpenRedirectUrl"]; }
        }

        public static string PayCenterDoamin
        {
            get { return ConfigurationManager.AppSettings["PayCenter_Doamin"].ToString();}
        }

        #region 艾米森
        public static string Aimisen_Finish_Url
        {
            get
            {
                return ConfigurationManager.AppSettings["pay_finish_url"].ToString();
            }
        }
        public static string Aimisen_Key
        {
            get { return "HBVoPuyEg61524047677AKm5bHWlJLu"; }
        }
        public static string Aimisen_PayURL
        {
            get { return "https://example.invalid/trade/pay"; }
        }
        public static string Aimisen_SrcCode
        {
            get { return "AMxtVB1524047677rultj"; }
        }
        public static string Aimisen_mchid
        {
            get { return "140011"; }
        }
        public static string Aimisen_logFile { get { return "Aimisen.log"; } }
        #endregion

        #region w付

        public static string Wfu_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_wfu_notifyurl"].ToString(); } }
        public static string Wfu_sign_type { get { return "RSA-S"; } }

        //商家代號
        public static string Wfu_merchant_code { get { return "501502002050"; } }

        //支付位置
        public static string Wfu_PayURL { get { return "https://example.invalid/gateway/api/h5apipay"; } }

        //商家私钥
        public static string Wfu_merchant_private_key { get { return "MIICeAIBADANBgkqhkiG9w0BAQEFAASCAmIwggJeAgEAAoGBAMXkMjw2sb6N2p2yT/o+CUd3ArDy0DDbRXudol78LgdUANW/og0eRfUGd8Ni9LYsc3+FYiBz87iVF5Uc2kbeTpEnmjO578G/9Pr4JHByZNkXtMFJfDfEH3n4oVh7+XNHA0r3P/sEaSd+b27ZK72BUkfy7gEyXMkaAaZsxQHNQ4MNAgMBAAECgYBKL99pVXPTJSj0/5keDk26PfKDVbQY7MsiqZjqlQ3Ib4SUnW/qDRRa5F7Xn+xvs0BlaSK9Wdrfsv0RSKzH7qVy+iKepXv2DuI8bDAzMfjey2f/pwqIqfeTx1G9E0mzWT+oHMyzJLldJO3DAuew00oXfM889ShBSvwL0OSrJw2b/QJBAPPDFwl6c+MztZdfowOlbToep46amjBxVVb58ANIFQz8rU0hQ9RqVLn28BI8tLe3tM58B1tvbVKrzmuq4SzMROMCQQDP04+t5in6jVNwqaJt0dh/b8lyQSf/GqqOXJ9MnsCBU8z/BWgpGk0fR14Fbw2CAD3RJ00D8BLGtOTM3Qa3UYtPAkEA8FzL54W+pifEBPrB9IN3vBqkIGLkbaYjf3s/Jw057K9vd6G4j8k1uJX9zg3cKVhVSlJUst2iECXnipVaKmFV/wJBAJ00pUyf/EUgvmtMhieYQrIzNyiMDNs1qqpBeeaVd3zcjQajamUdFhWBAKgPlyItWJVrqyA9a5aPozssPjZRIpUCQQC84CKzJudcQ03GoN/D1mJayKm4VTH5klFJEajpVkeK81xXXny8xEVjUSN0XY1ru1SNSauim7BE0nq1AFFgkHYs"; } }

        //Wfu公鑰
        public static string Wfu_public_key { get { return "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCcR1cV2j9AAMRq4qHjYTqljx+pUsvpUCyTnw4jSUrlfS6XvXrjbav3DsZRPVZ97s+As4/3Ms8L4/swJKdyQSPnyrzq7TDACAH/dN7YMyB2g13aD73QVxB/pB6/D71fB1P3Ypi/39pRejteQZXo9SBv3ASaQ+YhDzgUC9CXTSJbEQIDAQAB"; } }

        //W付公钥格式转换
        public static string Wfu_RSAPublicKeyJava2DotNet(string publicKey)
        {
            return RSAPublicKeyJava2DotNet(publicKey);
        }

        //使用W付公钥验签
        public static bool Wfu_ValidateRsaSign(string plainText, string publicKey, string signedData)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters para = new RSAParameters();
                rsa.FromXmlString(publicKey);
                return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(plainText), "md5", Convert.FromBase64String(signedData));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string Wfu_RSAPrivateKeyJava2DotNet(string privateKey)
        {
            return RSAPrivateKeyJava2DotNet(privateKey);
        }

        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        public static string RSAPrivateKeyJava2DotNet(string privateKey)
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

        //商户私钥签名
        public static string Wfu_RSASign(string signStr, string privateKey)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters para = new RSAParameters();
                rsa.FromXmlString(privateKey);
                byte[] signBytes = rsa.SignData(UTF8Encoding.UTF8.GetBytes(signStr), "md5");
                return Convert.ToBase64String(signBytes);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string Wfu_logFile { get { return "Wfu.log"; } }

        #endregion

        #region 70
        public static string Chishrka_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_chishrka_notifyurl"].ToString(); } }

        public static string Chishrka_Key
        {
            get { return "7dc1551a23f44ed4ac2ee15b5ac4bd47"; }
        }
        public static string Chishrka_PayURL
        {
            get { return "http://example.invalid/pay/gateway?"; }
        }
        public static string Chishrka_mchid
        {
            get { return "16653"; }
        }
        public static string Chishrka_logFile { get { return "Chishrka.log"; } }
        #endregion

        #region Tungle

        public static string Tungle_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_tungle_notifyurl"].ToString(); } }

        public static string Tungle_Key
        {
            get { return "389a95d0847bfcf0a127d4fa481480c7"; }
        }
        public static string Tungle_PayURL
        {
            get
            {
                return "http://example.invalid/gateway/soa";    //"http://example.invalid/gateway/soa";
            }

        }
        public static string Tungle_mchid
        {
            get { return "2105"; }
        }
        public static string Tungle_logFile { get { return "Tungle.log"; } }

        #endregion

        #region Jiuhe

        public static string Jiuhe_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_jiuhe_notifyurl"].ToString(); } }

        public static string Jiuhe_Key
        {
            get { return "zWvVJIzdkIRZg12AfCwe8KsU8YS33bRZ"; }
        }
        public static string Jiuhe_PayURL
        {
            get { return "http://example.invalid/pay/getway?"; }
        }
        public static string Jiuhe_mchid
        {
            get { return "20036"; }
        }
        public static string Jiuhe_logFile { get { return "Jiuhe.log"; } }

        #endregion

        #region tungyi

        public static string Tungyi_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_tungyi_url"].ToString(); }
        }

        public static string Tungyi_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_tungyi_mchid"].ToString(); }
        }

        public static string Tungyi_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tungyi_Key"].ToString(); }
        }

        public static string Shanfu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_shanfu_Key"].ToString(); }
        }

        public static string GetAmountList
        {
            get { return ConfigurationManager.AppSettings["GetAmountList"].ToString(); }
        }

        public static string Tungyi_logFile { get { return "Tungyi.log"; } }
        public static string AE_logFile { get { return "AE.log"; } }

        public static string TungyiEnable { get { return ConfigurationManager.AppSettings["TungyiEnable"].ToString(); } }

        #endregion

        #region 闪付

        public static string Shanfu_logFile { get { return "Shanfu.log"; } }

        #endregion

        #region MCPay

        public static string MCPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_mcpay_url"].ToString(); }
        }

        public static string MCPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_mcpay_Key"].ToString(); }
        }

        public static string MCPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_mcpay_callbackurl"].ToString(); }
        }

        public static string MCPay_logFile { get { return "MCPay.log"; } }

        #endregion

        #region XPay

        public static string XPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xpay_url"].ToString(); }
        }

        public static string XPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xpay_callbackurl"].ToString(); }
        }

        public static string XPay_mchId
        {
            get { return ConfigurationManager.AppSettings["xpay_mchId"].ToString(); }
        }

        public static string XPay_appId
        {
            get { return ConfigurationManager.AppSettings["xpay_appId"].ToString(); }
        }

        public static string XPay_key
        {
            get { return ConfigurationManager.AppSettings["pay_xpay_key"].ToString(); }
        }

        public static string XPay_logFile { get { return "XPay.log"; } }        

        #endregion

        #region Xunpay

        public static string Xunpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_Xunpay_url"].ToString(); }
        }

        public static string Xunpay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_Xunpay_callbackurl"].ToString(); }
        }

        public static string Xunpay_appId
        {
            get { return ConfigurationManager.AppSettings["Xunpay_appId"].ToString(); }
        }

        public static string Xunpay_key
        {
            get { return ConfigurationManager.AppSettings["Xunpay_key"].ToString(); }
        }

        public static string Xunpay_logFile { get { return "Xunpay.log"; } }

        #endregion

        #region CfanPay

        public static string CfanPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_cfanpay_url"].ToString(); }
        }

        public static string CfanPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_cfanpay_callbackurl"].ToString(); }
        }

        public static string CfanPay_mchId
        {
            get { return ConfigurationManager.AppSettings["cfanpay_mchId"].ToString(); }
        }

        public static string CfanPay_key
        {
            get { return ConfigurationManager.AppSettings["cfanpay_key"].ToString(); }
        }

        public static string CfanPay_logFile { get { return "CfanPay.log"; } }

        #endregion

        #region 快聯


        public static string kuailian_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_kuailian_url"].ToString(); }
        }


        public static string kuailian_Key
        {
            get { return ConfigurationManager.AppSettings["pay_kuailian_Key"].ToString(); }
        }

        public static string kuailian_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_kuailian_mchid"].ToString(); }
        }
        public static string kuailian_notifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kuailian_notifyUrl"].ToString(); }
        }


        public static string kuailian_logFile { get { return "kuailian.log"; } }

        #endregion

        #region UNPay

        public static string UNPay_Parter
        {
            get { return ConfigurationManager.AppSettings["pay_unpay_parter"].ToString(); }
        }

        public static string UNPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_unpay_url"].ToString(); }
        }

        public static string UNPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_unpay_key"].ToString(); }
        }

        public static string UNPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_unpay_callbackurl"].ToString(); }
        }

        public static string UNPay_logFile { get { return "UNPay.log"; } }
        #endregion

        #region 店員通

        public static string dianyuantong_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_dianyuantong_url"].ToString(); }
        }

        public static string dianyuantong_Key
        {
            get { return ConfigurationManager.AppSettings["pay_dianyuantong_Key"].ToString(); }
        }

        public static string dianyuantong_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_dianyuantong_mchid"].ToString(); }
        }
        public static string dianyuantong_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_dianyuantong_callbackUrl"].ToString(); }
        }

        public static string dianyuantong_logFile { get { return "dianyuantong.log"; } }

        #endregion

        #region 财付

        public static string xf188_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_188xf_mchid"].ToString(); }
        }

        public static string xf188_Key
        {
            get { return ConfigurationManager.AppSettings["pay_188xf_Key"].ToString(); }
        }

        public static string xf188_Pkid
        {
            get { return ConfigurationManager.AppSettings["pay_188xf_Pkid"].ToString(); }
        }

        public static string xf188_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_188xf_url"].ToString(); }
        }
        public static string xf188_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_188xfg_callbackUrl"].ToString(); }
        }
        public static string xf188_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_188xfg_notifyUrl"].ToString(); }
        }

        public static string xf188_logFile { get { return "xf188.log"; } }

        #endregion

        #region 亿点

        public static string gpyd_merchantNum
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_merchantNum"].ToString(); }
        }

        public static string gpyd__Key
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_Key"].ToString(); }
        }

        public static string gpyd_merMark
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_merMark"].ToString(); }
        }

        public static string gpyd_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_url"].ToString(); }
        }
        public static string gpyd_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_callbackUrl"].ToString(); }
        }
        public static string gpyd_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_gpyd_notifyUrl"].ToString(); }
        }

        public static string gpyd_logFile { get { return "gpyd.log"; } }

        #endregion

        #region 客付通

        public static string kft_platMerId
        {
            get { return ConfigurationManager.AppSettings["pay_kft_platMerId"].ToString(); }
        }

        public static string kft_Key
        {
            get { return ConfigurationManager.AppSettings["pay_kft_Key"].ToString(); }
        }

        public static string kft_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_kft_url"].ToString(); }
        }
        public static string kft_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kft_notifyUrl"].ToString(); }
        }

        public static string kft_logFile { get { return "kftpay.log"; } }

        #endregion

        #region 汇源通

        public static string hyt_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_hyt_memberid"].ToString(); }
        }

        public static string hyt_Key
        {
            get { return ConfigurationManager.AppSettings["pay_hyt_Key"].ToString(); }
        }

        public static string hyt_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_hyt_url"].ToString(); }
        }
        public static string hyt_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hyt_notifyUrl"].ToString(); }
        }
        public static string hyt_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hyt_callbackurl"].ToString(); }
        }

        public static string hyt_logFile { get { return "hytpay.log"; } }

        #endregion

        #region 星入宝

        public static string xingrubao_uid
        {
            get { return ConfigurationManager.AppSettings["pay_xingrubao_uid"].ToString(); }
        }

        public static string xingrubao_token
        {
            get { return ConfigurationManager.AppSettings["pay_xingrubao_token"].ToString(); }
        }

        public static string xingrubao_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xingrubao_url"].ToString(); }
        }
        public static string xingrubao_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingrubao_notifyUrl"].ToString(); }
        }
        public static string xingrubao_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingrubao_callbackurl"].ToString(); }
        }

        public static string xingrubao_logFile { get { return "xingrubao.log"; } }

        #endregion

        #region 永安

        public static string ya_code
        {
            get { return ConfigurationManager.AppSettings["pay_ya_code"].ToString(); }
        }

        public static string ya_key
        {
            get { return ConfigurationManager.AppSettings["pay_ya_key"].ToString(); }
        }

        public static string ya_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_ya_url"].ToString(); }
        }
        public static string ya_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ya_notifyUrl"].ToString(); }
        }
        public static string ya_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ya_callbackurl"].ToString(); }
        }

        public static string ya_logFile { get { return "yapay.log"; } }

        #endregion

        #region 融支付

        public static string rong_channelno
        {
            get { return ConfigurationManager.AppSettings["pay_rong_channelno"].ToString(); }
        }

        public static string rong_key
        {
            get { return ConfigurationManager.AppSettings["pay_rong_key"].ToString(); }
        }

        public static string rong_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_rong_url"].ToString(); }
        }
        public static string rong_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_rong_callbackurl"].ToString(); }
        }

        public static string rong_logFile { get { return "rongpay.log"; } }

        #endregion

        #region 维护宝

        public static string whb_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_whb_memberid"].ToString(); }
        }

        public static string whb_Key
        {
            get { return ConfigurationManager.AppSettings["pay_whb_Key"].ToString(); }
        }

        public static string whb_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_whb_url"].ToString(); }
        }
        public static string whb_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_whb_notifyUrl"].ToString(); }
        }
        public static string whb_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_whb_callbackurl"].ToString(); }
        }

        public static string whb_logFile { get { return "whbpay.log"; } }

        #endregion

        #region 8pay

        public static string eightpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_eightpay_memberid"].ToString(); }
        }

        public static string eightpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_eightpay_Key"].ToString(); }
        }

        public static string eightpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_eightpay_url"].ToString(); }
        }
        public static string eightpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_eightpay_notifyUrl"].ToString(); }
        }
        public static string eightpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_eightpay_callbackurl"].ToString(); }
        }

        public static string eightpay_logFile { get { return "eightpay.log"; } }

        #endregion

        #region 聚眾

        public static string juzhongpay_merId
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_merId"].ToString(); }
        }
        public static string juzhongpay_terId
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_terId"].ToString(); }
        }

        public static string juzhongpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_Key"].ToString(); }
        }

        public static string juzhongpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_url"].ToString(); }
        }
        public static string juzhongpay_syncURL
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_syncURL"].ToString(); }
        }
        public static string juzhongpay_asynURL
        {
            get { return ConfigurationManager.AppSettings["pay_juzhongpay_asynURL"].ToString(); }
        }

        public static string juzhongpay_logFile { get { return "juzhongpay.log"; } }

        #endregion

        #region 万伽

        public static string wangyuapay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_wangyuapay_mchid"].ToString(); }
        }

        public static string wangyuapay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_wangyuapay_Key"].ToString(); }
        }

        public static string wangyuapay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_wangyuapay_url"].ToString(); }
        }
      
        public static string wangyuapay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_wangyuapay_notifyUrl"].ToString(); }
        }

        public static string wangyuapay_logFile { get { return "wangyuapay.log"; } }

        #endregion

        #region 天边

        public static string tianbianpay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_tianbianpay_mchid"].ToString(); }
        }

        public static string tianbianpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tianbianpay_Key"].ToString(); }
        }

        public static string tianbianpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_tianbianpay_url"].ToString(); }
        }

        public static string tianbianpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_tianbianpay_notifyUrl"].ToString(); }
        }

        public static string tianbianpay_logFile { get { return "tianbianpay.log"; } }

        #endregion

        #region 派派

        public static string ppzfpay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_ppzfpay_mchid"].ToString(); }
        }

        public static string ppzfpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_ppzfpay_Key"].ToString(); }
        }

        public static string ppzfpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_ppzfpay_url"].ToString(); }
        }

        public static string ppzfpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ppzfpay_notifyUrl"].ToString(); }
        }
        public static string ppzfpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ppzfpay_callbackurl"].ToString(); }
        }

        public static string ppzfpay_logFile { get { return "ppzfpay.log"; } }

        #endregion

        #region 凯旋

        public static string kaixuanpay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_kaixuanpay_mchid"].ToString(); }
        }

        public static string kaixuanpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_kaixuanpay_Key"].ToString(); }
        }

        public static string kaixuanpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_kaixuanpay_url"].ToString(); }
        }

        public static string kaixuanpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kaixuanpay_notifyUrl"].ToString(); }
        }
        public static string kaixuanpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kaixuanpay_callbackurl"].ToString(); }
        }

        public static string kaixuanpay_logFile { get { return "kaixuanpay.log"; } }

        #endregion

        #region 盈支付

        public static string yinpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_yinpay_memberid"].ToString(); }
        }

        public static string yinpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_yinpay_Key"].ToString(); }
        }

        public static string yinpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_yinpay_url"].ToString(); }
        }
        public static string yinpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_yinpay_notifyUrl"].ToString(); }
        }
        public static string yinpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_yinpay_callbackurl"].ToString(); }
        }

        public static string yinpay_logFile { get { return "yinpay.log"; } }

        #endregion

        #region 快付支付

        public static string kuaifu_partner
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_partner"].ToString(); }
        }
        public static string kuaifu_seller
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_seller"].ToString(); }
        }

        public static string kuaifu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_Key"].ToString(); }
        }

        public static string kuaifu_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_url"].ToString(); }
        }
        public static string kuaifu_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_notifyUrl"].ToString(); }
        }
        public static string kuaifu_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_kuaifu_callbackurl"].ToString(); }
        }

        public static string kuaifu_logFile { get { return "kuaifupay.log"; } }

        #endregion

        #region 牛牛支付

        public static string niuniu_loginid
        {
            get { return ConfigurationManager.AppSettings["pay_niuniu_loginid"].ToString(); }
        }
        public static string niuniu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_niuniu_Key"].ToString(); }
        }

        public static string niuniu_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_niuniu_url"].ToString(); }
        }
        public static string niuniu_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_niuniu_notifyUrl"].ToString(); }
        }
      

        public static string niuniu_logFile { get { return "niuniupay.log"; } }

        #endregion

        #region 盒付支付

        public static string hefupay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_hefupay_memberid"].ToString(); }
        }

        public static string hefupay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_hefupay_Key"].ToString(); }
        }

        public static string hefupay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_hefupay_url"].ToString(); }
        }
        public static string hefupay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hefupay_notifyUrl"].ToString(); }
        }
        public static string hefupay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hefupay_callbackurl"].ToString(); }
        }

        public static string hefupay_logFile { get { return "hefupay.log"; } }

        #endregion

        #region 慧云支付

        public static string huiyun_customerid
        {
            get { return ConfigurationManager.AppSettings["pay_huiyun_customerid"].ToString(); }
        }
        public static string huiyun_Key
        {
            get { return ConfigurationManager.AppSettings["pay_huiyun_Key"].ToString(); }
        }

        public static string huiyun_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_huiyun_url"].ToString(); }
        }
        public static string huiyun_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_huiyun_notifyUrl"].ToString(); }
        }
        public static string huiyun_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_huiyun_callbackurl"].ToString(); }
        }


        public static string huiyun_logFile { get { return "huiyunpay.log"; } }

        #endregion

        #region 多聚发支付

        public static string djfzf_partner
        {
            get { return ConfigurationManager.AppSettings["pay_djfzf_partner"].ToString(); }
        }
        public static string djfzf_Key
        {
            get { return ConfigurationManager.AppSettings["pay_djfzf_Key"].ToString(); }
        }

        public static string djfzf_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_djfzf_url"].ToString(); }
        }
        public static string djfzf_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_djfzf_notifyUrl"].ToString(); }
        }
        public static string djfzf_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_djfzf_callbackurl"].ToString(); }
        }


        public static string djfzf_logFile { get { return "djfzfpay.log"; } }

        #endregion

        #region 环信2站

        public static string hx2z_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_hx2z_memberid"].ToString(); }
        }

        public static string hx2z_Key
        {
            get { return ConfigurationManager.AppSettings["pay_hx2z_Key"].ToString(); }
        }

        public static string hx2z_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_hx2z_url"].ToString(); }
        }
        public static string hx2z_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hx2z_notifyUrl"].ToString(); }
        }
        public static string hx2z_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hx2z_callbackurl"].ToString(); }
        }

        public static string hx2z_logFile { get { return "hx2zpay.log"; } }

        #endregion

        #region 金牌 MCPAY

        public static string gpmc_appid
        {
            get { return ConfigurationManager.AppSettings["pay_gpmc_appid"].ToString(); }
        }

        public static string gpmc_Key
        {
            get { return ConfigurationManager.AppSettings["pay_gpmc_Key"].ToString(); }
        }

        public static string gpmc_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_gpmc_url"].ToString(); }
        }
        public static string gpmc_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_gpmc_callbackurl"].ToString(); }
        }

        public static string gpmc_logFile { get { return "gpmcpay.log"; } }

        #endregion

        #region 宝盒支付

        public static string Baohepay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_baohepay_url"].ToString(); }
        }

        public static string Baohepay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_baohepay_callbackurl"].ToString(); }
        }

        public static string Baohepay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_baohepay_notifyurl"].ToString(); }
        }

        public static string Baohepay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_baohepay_mchid"].ToString(); }
        }

        public static string Baohepay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_baohepay_key"].ToString(); }
        }        

        public static string Baohepay_logFile { get { return "Baohepay.log"; } }

        #endregion

        #region 铁锤支付

        public static string Tiechuipay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_url"].ToString(); }
        }

        public static string Tiechuipay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_callbackurl"].ToString(); }
        }       

        public static string Tiechuipay_merchantCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_merchantCode"].ToString(); }
        }

        public static string Tiechuipay_agentCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_agentCode"].ToString(); }
        }

        public static string Tiechuipay_channelCode
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_channelCode"].ToString(); }
        }

        public static string Tiechuipay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_key"].ToString(); }
        }

        public static string Tiechuipay_RSA_PrivateKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PrivateKey"].ToString(); }
        }

        public static string Tiechuipay_RSA_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_tiechuipay_RSA_PublicKey"].ToString(); }
        }

        public static string Tiechuipay_logFile { get { return "Tiechuipay.log"; } }

        #endregion

        #region 冬瓜支付

        public static string Xiamipay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_Xiamipay_url"].ToString(); }
        }

        public static string Xiamipay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_Xiamipay_callbackurl"].ToString(); }
        }

        public static string Xiamipay_appKey
        {
            get { return ConfigurationManager.AppSettings["pay_Xiamipay_appKey"].ToString(); }
        }

        public static string Xiamipay_appSecret
        {
            get { return ConfigurationManager.AppSettings["pay_Xiamipay_appSecret"].ToString(); }
        }       

        public static string Xiamipay_logFile { get { return "Xiamipay.log"; } }

        #endregion

        #region 蜻蜓

        public static string Qingting_URL
        {
            get { return ConfigurationManager.AppSettings["pay_Qingting_url"].ToString(); }
        }

        public static string Qingting_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_qingting_callbackurl"].ToString(); }
        }       

        public static string Qingting_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_qingting_mchid"].ToString(); }
        }

        public static string Qingting_Key
        {
            get { return ConfigurationManager.AppSettings["pay_qingting_key"].ToString(); }
        }

        public static string QingtingQe_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_qingtingqe_mchid"].ToString(); }
        }

        public static string QingtingQe_Key
        {
            get { return ConfigurationManager.AppSettings["pay_qingtingqe_key"].ToString(); }
        }

        public static string Qingting_logFile { get { return "Qingting.log"; } }

        #endregion

        #region 汇通

        public static string Huitong_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_huitong_url"].ToString(); }
        }

        public static string Huitong_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_huitong_callbackurl"].ToString(); }
        }

        public static string Huitong_mchId
        {
            get { return ConfigurationManager.AppSettings["huitong_mchId"].ToString(); }
        }

        public static string Huitong_appId
        {
            get { return ConfigurationManager.AppSettings["huitong_appId"].ToString(); }
        }

        public static string Huitong_key
        {
            get { return ConfigurationManager.AppSettings["pay_huitong_key"].ToString(); }
        }

        public static string Huitong_logFile { get { return "Huitong.log"; } }

        #endregion

        #region Rongyin

        public static string Rongyin_code
        {
            get { return ConfigurationManager.AppSettings["pay_rongyin_code"].ToString(); }
        }

        public static string Rongyin_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_rongyin_url"].ToString(); }
        }

        public static string Rongyin_Key
        {
            get { return ConfigurationManager.AppSettings["pay_rongyin_key"].ToString(); }
        }

        public static string Rongyin_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_rongyin_callbackurl"].ToString(); }
        }

        public static string Rongyin_logFile { get { return "Rongyin.log"; } }
        #endregion

        #region U鑫

        public static string Uxin_URL
        {
            get { return ConfigurationManager.AppSettings["pay_uxin_url"].ToString(); }
        }

        public static string Uxin_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_uxin_callbackurl"].ToString(); }
        }

        public static string Uxin_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_uxin_notifyurl"].ToString(); }
        }

        public static string Uxin_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_uxin_mchid"].ToString(); }
        }

        public static string Uxin_Key
        {
            get { return ConfigurationManager.AppSettings["pay_uxin_key"].ToString(); }
        }

        public static string Uxin_logFile { get { return "Uxin.log"; } }


        #endregion

        #region 萬馬

        public static string HiPay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_url"].ToString(); }
        }

        public static string HiPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_callbackurl"].ToString(); }
        }

        public static string HiPay_account
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_account"].ToString(); }
        }

        public static string HiPay_merchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_merchantNo"].ToString(); }
        }           

        public static string HiPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_key"].ToString(); }
        }       

        public static string HiPay_RSA_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_hipay_RSA_PublicKey"].ToString(); }
        }

        public static string HiPay_logFile { get { return "HiPay.log"; } }

        #endregion

        #region 安迅通支付

        public static string Anxanpay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_anxanpay_url"].ToString(); }
        }

        public static string Anxanpay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_anxanpay_callbackurl"].ToString(); }
        }

        public static string Anxanpay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_anxanpay_notifyurl"].ToString(); }
        }

        public static string Anxanpay_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_anxanpay_mchid"].ToString(); }
        }

        public static string Anxanpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_anxanpay_key"].ToString(); }
        }

        public static string Anxanpay_logFile { get { return "Anxanpay.log"; } }

        #endregion

        #region 365支付

        public static string Pay365_Url { get { return ConfigurationManager.AppSettings["pay_365pay_url"].ToString(); } }
        public static string Pay365_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_365pay_notifyurl"].ToString(); } }
        public static string Pay365_PartnerID { get { return ConfigurationManager.AppSettings["pay_365pay_partnerid"].ToString(); } }
        public static string Pay365_Key { get { return ConfigurationManager.AppSettings["pay_365pay_key"].ToString(); } }
        public static string Pay365_logFile { get { return "365Pay.log"; } }

        #endregion

        #region 闪入宝
        public static string ShanRuBao_Url { get { return ConfigurationManager.AppSettings["pay_shanrubao_url"].ToString(); } }
        public static string ShanRuBao_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_shanrubao_notifyurl"].ToString(); } }
        public static string ShanRuBao_ReturnUrl { get { return ConfigurationManager.AppSettings["pay_shanrubao_returnurl"].ToString(); } }
        public static string ShanRuBao_UID { get { return ConfigurationManager.AppSettings["pay_shanrubao_uid"].ToString(); } }
        public static string ShanRuBao_Token { get { return ConfigurationManager.AppSettings["pay_shanrubao_token"].ToString(); } }
        public static string ShanRuBao_logFile { get { return "ShanRuBao.log"; } }

        #endregion

        #region 金城支付
        public static string JinCheng_Url { get { return ConfigurationManager.AppSettings["pay_jincheng_url"].ToString(); } }
        public static string JinCheng_NotifyUrl { get { return ConfigurationManager.AppSettings["pay_jincheng_notifyurl"].ToString(); } }
        public static string JinCheng_ReturnUrl { get { return ConfigurationManager.AppSettings["pay_jincheng_returnurl"].ToString(); } }
        public static string JinCheng_MerchantCode { get { return ConfigurationManager.AppSettings["pay_jincheng_merchantcode"].ToString(); } }
        public static string JinCheng_Key { get { return ConfigurationManager.AppSettings["pay_jincheng_key"].ToString(); } }
        public static string JinCheng_logFile { get { return "JinCheng.log"; } }

        #endregion

        #region AE支付
        public static string AEPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_ae_url"].ToString(); }
        }
        public static string AEPay_AUTH
        {
            get { return ConfigurationManager.AppSettings["pay_ae_auth"].ToString(); }
        }
        public static string AEPay_CallbackUrl
        {
            get { return FinishUrl; }
        }

        public static string AEPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_ae_notifyurl"].ToString(); }
        }
        public static string AEPay_logFile
        {
            get { return "AEPay.log"; }
        }
        public static string AEPayCallback_logFile
        {
            get { return "AEPayCallback.log"; }
        }
        #endregion

        #region 王朝金流
        public static string DynasPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_dynas_url"].ToString(); }
        }
        public static string DynasPay_HashKey
        {
            get { return ConfigurationManager.AppSettings["pay_dynas_hashkey"].ToString(); }
        }
        public static string DynasPay_HashIv
        {
            get { return ConfigurationManager.AppSettings["pay_dynas_hashiv"].ToString(); }
        }
        public static string DynasPay_VerifyCode
        {
            get { return ConfigurationManager.AppSettings["pay_dynas_verifycode"].ToString(); }
        }
        public static string DynasPay_CallbackUrl
        {
            get { return FinishUrl; }
        }

        public static string DynasPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_dynas_notifyurl"].ToString(); }
        }
        public static string DynasPay_logFile
        {
            get { return "DynasPay.log"; }
        }
        #endregion

        #region 碼上賺

        public static string MSZPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_msz_notifyurl"].ToString(); }
        }
        public static string MSZPay_Prefix
        {
            get { return ConfigurationManager.AppSettings["pay_msz_prefix"].ToString(); }
        }
        public static string MSZPay_logFile
        {
            get { return "MSZPay.log"; }
        }
        public static string MSZBankWithdraw_Prefix
        {
            get { return ConfigurationManager.AppSettings["withdraw_msz_prefix"].ToString(); }
        }
        public static string MSZBankWithdraw_Merchant_Code
        {
            get { return ConfigurationManager.AppSettings["withdraw_msz_merchant_code"].ToString(); }
        }
        public static string MSZBankWithdraw_Secure_Key
        {
            get { return ConfigurationManager.AppSettings["withdraw_msz_secure_key"].ToString(); }
        }
        public static string MSZBankWithdraw_logFile
        {
            get { return "MSZBankWithdraw.log"; }
        }
        #endregion

        #region 商易付

        public static string SYFPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_syf_notifyurl"].ToString(); }
        }

        public static string SYFPay_logFile
        {
            get { return "SYFPay.log"; }
        }
        #endregion

        #region 新威付
        /// <summary>
        /// 新威付服务端通知:服务端返回地址
        /// </summary>
        public static string NewWeiFuPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_NewWeiFuPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 新威付Log檔案名稱
        /// </summary>
        public static string NewWeiFuPay_logFile
        {
            get { return "NewWeiFuPay.log"; }
        }

        #endregion

        #region 巨富
        /// <summary>
        /// 巨富服务端通知:服务端返回地址
        /// </summary>
        public static string GiFuBoPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_GiFuBoPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 巨富Log檔案名稱
        /// </summary>
        public static string GiFuBoPay_logFile
        {
            get { return "GiFuBoPay.log"; }
        }

        #endregion

        public static string FinishUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_finishUrl"].ToString(); }
        }

        #region 叮叮寶
        /// <summary>
        /// 叮叮寶服务端通知:服务端返回地址
        /// </summary>
        public static string DDPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_DDPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 叮叮寶Log檔案名稱
        /// </summary>
        public static string DDPay_logFile
        {
            get { return "DDPay.log"; }
        }
        #endregion

        #region 通匯
        /// <summary>
        /// 通匯服务端通知:服务端返回地址
        /// </summary>
        public static string THPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_THPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 通匯Log檔案名稱
        /// </summary>
        public static string THPay_logFile
        {
            get { return "THPay.log"; }
        }
        #endregion

        #region 鐵付
        /// <summary>
        /// 鐵付服务端通知:服务端返回地址
        /// </summary>
        public static string IronPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_IronPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 鐵付Log檔案名稱
        /// </summary>
        public static string IronPay_logFile
        {
            get { return "IronPay.log"; }
        }
        #endregion

        #region 四方(新王朝)
        /// <summary>
        /// 四方(新王朝)服务端通知:服务端返回地址
        /// </summary>
        public static string HFZCPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_HFZCPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 四方(新王朝)Log檔案名稱
        /// </summary>
        public static string HFZCPay_logFile
        {
            get { return "HFZCPay.log"; }
        }
        #endregion

        #region 啟飛
        /// <summary>
        /// 啟飛服务端通知:服务端返回地址
        /// </summary>
        public static string GoldSoarPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_GoldSoarPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 啟飛Log檔案名稱
        /// </summary>
        public static string GoldSoarPay_logFile
        {
            get { return "GoldSoarPay.log"; }
        }
        #endregion

        #region 吉寶
        /// <summary>
        /// 吉寶服务端通知:服务端返回地址
        /// </summary>
        public static string JBPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_JBPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string JBPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_JBPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 吉寶Log檔案名稱
        /// </summary>
        public static string JBPay_logFile
        {
            get { return "JBPay.log"; }
        }
        #endregion

        #region 樂匯第三方
        /// <summary>
        /// 樂匯第三方服务端通知:服务端返回地址
        /// </summary>
        public static string LeHuiPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_LeHuiPay_notifyUrl"].ToString(); }
        }

        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string LeHuiPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_LeHuiPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 樂匯第三方Log檔案名稱
        /// </summary>
        public static string LeHuiPay_logFile
        {
            get { return "LeHuiPay.log"; }
        }
        #endregion

        #region 第一付
        /// <summary>
        /// 第一付服务端通知:服务端返回地址
        /// </summary>
        public static string D1FPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_D1FPay_notifyUrl"].ToString(); }
        }

        /// <summary>
        /// 第一付創建訂單Api的URL
        /// </summary>
        public static string D1FPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_D1FPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 第一付Log檔案名稱
        /// </summary>
        public static string D1FPay_logFile
        {
            get { return "D1FPay.log"; }
        }
        #endregion

        #region 立馬付
        /// <summary>
        /// 立馬付服务端通知:服务端返回地址
        /// </summary>
        public static string LiMaFuPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_LiMaFuPay_notifyUrl"].ToString(); }
        }

        /// <summary>
        /// 立馬付Log檔案名稱
        /// </summary>
        public static string LiMaFuPay_logFile
        {
            get { return "LiMaFuPay.log"; }
        }

        #endregion

        #region 永勝支付
        /// <summary>
        /// 永勝支付服务端通知:服务端返回地址
        /// </summary>
        public static string YungShengPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_YungShengPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string YungShengPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_YungShengPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 永勝支付Log檔案名稱
        /// </summary>
        public static string YungShengPay_logFile
        {
            get { return "YungShengPay.log"; }
        }
        #endregion

        #region 永信支付
        public static string YongXinPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_YongXinPay_createOrderApiUrl"].ToString(); }
        }

        public static string YongXinPay_NotifyUrl
        {

            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_YongXinPay_notifyurl"].ToString(); }
        }

        public static string YongXinPay_logFile
        {
            get { return "YongXinPay.log"; }
        }
        #endregion

        #region 微支付
        /// <summary>
        /// 微支付服务端通知:服务端返回地址
        /// </summary>
        public static string WpPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_WpPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string WpPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_WpPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 微支付Log檔案名稱
        /// </summary>
        public static string WpPay_logFile
        {
            get { return "WpPay.log"; }
        }
        #endregion

        #region DFQJ支付
        /// <summary>
        /// DFQJ支付服务端通知:服务端返回地址
        /// </summary>
        public static string DfqjPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_DfqjPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string DfqjPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_DfqjPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// DFQJ支付Log檔案名稱
        /// </summary>
        public static string DfqjPay_logFile
        {
            get { return "DfqjPay.log"; }
        }
        #endregion

        #region SJQP支付
        /// <summary>
        /// SJQP支付服务端通知:服务端返回地址
        /// </summary>
        public static string SjqpPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_SjqpPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 創建訂單Api的URL
        /// </summary>
        public static string SjqpPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_SjqpPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// SJQP支付Log檔案名稱
        /// </summary>
        public static string SjqpPay_logFile
        {
            get { return "SjqpPay.log"; }
        }
        #endregion

        #region 光速付
        /// <summary>
        /// 光速付創建訂單Api的URL
        /// </summary>
        public static string SpeedPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_SpeedPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 光速付Log檔案名稱
        /// </summary>
        public static string SpeedPay_logFile
        {
            get { return "SpeedPay.log"; }
        }
        #endregion

        #region 善付
        /// <summary>
        /// 善付服务端通知:服务端返回地址
        /// </summary>
        public static string SFPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_SFPay_notifyUrl"].ToString(); }
        }

        /// <summary>
        /// 善付創建訂單Api的URL
        /// </summary>
        public static string SFPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_SFPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 善付Log檔案名稱
        /// </summary>
        public static string SFPay_logFile
        {
            get { return "SFPay.log"; }
        }
        #endregion

        #region 天空付
        /// <summary>
        /// 天空付服务端通知:服务端返回地址
        /// </summary>
        public static string WonderfulskyPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_WonderfulskyPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 天空付創建訂單Api的URL
        /// </summary>
        public static string WonderfulskyPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_WonderfulskyPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 天空付Log檔案名稱
        /// </summary>
        public static string WonderfulskyPay_logFile
        {
            get { return "WonderfulskyPay.log"; }
        }
        #endregion

        #region 佰坊支付
        /// <summary>
        /// 佰坊支付服务端通知:服务端返回地址
        /// </summary>
        public static string GoPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_GoPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 佰坊支付創建訂單Api的URL
        /// </summary>
        public static string GoPay_CreateOrderApiUrl
        {
            get { return "?c=Pay&a=index&n=1"; }
        }

        /// <summary>
        /// 佰坊支付Log檔案名稱
        /// </summary>
        public static string GoPay_logFile
        {
            get { return "GoPay.log"; }
        }
        #endregion

        #region 上分通
        /// <summary>
        /// 上分通服务端通知:服务端返回地址
        /// </summary>
        public static string SFTPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_SFTPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// 上分通創建訂單Api的URL
        /// </summary>
        public static string SFTPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_SFTPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 上分通Log檔案名稱
        /// </summary>
        public static string SFTPay_logFile
        {
            get { return "SFTPay.log"; }
        }
        #endregion

        #region 风云聚合
        /// <summary>
        /// 风云聚合創建訂單Api的URL
        /// </summary>
        public static string DSDFPay_CreateOrderApiUrl
        {
            get { return ConfigurationManager.AppSettings["pay_DSDFPay_createOrderApiUrl"].ToString(); }
        }

        /// <summary>
        /// 风云聚合Log檔案名稱
        /// </summary>
        public static string DSDFPay_logFile
        {
            get { return "DSDFPay.log"; }
        }
        /// <summary>
        /// 风云聚合-代付Log檔案名稱
        /// </summary>
        public static string DSDFPayWithdraw_logFile
        {
            get { return "DSDFPayWithdraw.log"; }
        }
        #endregion

        #region 利鑫
        /// <summary>
        /// 
        /// </summary>
        public static string LXPay_key
        {
            get { return ConfigurationManager.AppSettings["pay_LXPay_key"].ToString(); }
        }

        /// <summary>
        /// 利鑫 - 代付 prefix
        /// </summary>
        public static string LXPay_prefix
        {
            get { return ConfigurationManager.AppSettings["pay_LXPAY_prefix"].ToString(); }
        }

        /// <summary>
        /// 利鑫-代付Log檔案名稱
        /// </summary>
        public static string LXPayWithdraw_logFile
        {
            get { return "LXPayWithdraw.log"; }
        }
        #endregion

        #region 聚宝盆
        public static string MTPay_notifyurl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_MTPay_notifyurl"].ToString(); }
        }
        /// <summary>
        /// 訂單前綴
        /// </summary>
        public static string MTPay_perfix
        {
            get { return ConfigurationManager.AppSettings["pay_MTPay_prefix"].ToString(); }
        }
        /// <summary>
        /// 聚宝盆 Log檔案名稱
        /// </summary>
        public static string MTPay_logFile
        {
            get { return "MTPay.log"; }
        }
        /// <summary>
        /// MT代付
        /// </summary>
        public static string MTPay_withdraw_key
        {
            get { return ConfigurationManager.AppSettings["pay_MTPay_withdraw_key"].ToString(); }
        }

        /// <summary>
        /// 聚宝盆 withdraw Log檔案名稱
        /// </summary>
        public static string MTPayWithdraw_logFile
        {
            get { return "MTPayWithdraw.log"; }
        }
        #endregion

        #region TJPay

        public static string TJPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_TJPay_notifyurl"].ToString(); }
        }

        /// <summary>
        /// TJPay Log檔案名稱
        /// </summary>
        public static string TJPay_LogFile
        {
            get { return "TJPay.log"; }
        }
        #endregion

        #region 龍聚
        public static string LongJuPay_NotifyUrl
        {
            get { return PayCenterDoamin + ConfigurationManager.AppSettings["pay_LongJuPay_NotifyUrl"].ToString(); }
        }
        public static string LongJuPay_Prefix
        {
            get { return ConfigurationManager.AppSettings["pay_LongJuPay_Prefix"].ToString(); }
        }

        /// <summary>
        /// LongJuPay Log檔案名稱
        /// </summary>
        public static string LongJuPay_LogFile
        {
            get { return "LongJuPay.log"; }
        }
        #endregion

        /// <summary>
        /// 来源类型
        /// </summary>
        public enum PayState : byte
        {
            Success = 1,
            Error = 2,
            SignError = 3,
            Watting = 4
        }

    }


}