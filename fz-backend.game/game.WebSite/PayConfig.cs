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

        public static string WebSiteHost
        {
            get { return ConfigurationManager.AppSettings["WebSiteHost"]; }
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
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
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

        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
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

        public static string TungyiEnable { get { return ConfigurationManager.AppSettings["TungyiEnable"].ToString(); } }

        #endregion

        #region 轰达統一
        
        public static string TungyiHonda_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_tungyiHonda_mchid"].ToString(); }
        }

        public static string TungyiHonda_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tungyiHonda_Key"].ToString(); }
        }

        public static string TungyiHonda_logFile { get { return "TungyiHonda.log"; } }

        #endregion

        #region 巅峰統一

        public static string TungyiDF_mchid
        {
            get { return ConfigurationManager.AppSettings["pay_tungyiDF_mchid"].ToString(); }
        }

        public static string TungyiDF_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tungyiDF_Key"].ToString(); }
        }

        public static string TungyiDF_logFile { get { return "TungyiDF.log"; } }

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

        public static string Withdraw_ppzfpay_mchid
        {
            get { return ConfigurationManager.AppSettings["withdraw_ppzfpay_mchid"].ToString(); }
        }

        public static string Withdraw_ppzfpay_Key
        {
            get { return ConfigurationManager.AppSettings["withdraw_ppzfpay_Key"].ToString(); }
        }

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

        #region 久付支付

        public static string eightzpay_merchantid
        {
            get { return ConfigurationManager.AppSettings["pay_8zpay_merchantid"].ToString(); }
        }
        public static string eightzpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_8zpay_Key"].ToString(); }
        }

        public static string eightzpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_8zpay_url"].ToString(); }
        }
        public static string eightzpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_8zpay_notifyUrl"].ToString(); }
        }
        public static string eightzpay_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_8zpay_callbackurl"].ToString(); }
        }


        public static string eightzpay_logFile { get { return "8zpaypay.log"; } }

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
        
        #region 惠诚支付

        public static string huichengpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_huichengpay_memberid"].ToString(); }
        }

        public static string huichengpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_huichengpay_Key"].ToString(); }
        }

        public static string huichengpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_huichengpay_url"].ToString(); }
        }
        public static string huichengpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_huichengpay_notifyUrl"].ToString(); }
        }
        public static string huichengpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_huichengpay_callbackurl"].ToString(); }
        }

        public static string huichengpay_logFile { get { return "huichengpay.log"; } }
        #endregion

        #region 新金鑫支付

        public static string jinxinpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_jinxinpay_memberid"].ToString(); }
        }

        public static string jinxinpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_jinxinpay_Key"].ToString(); }
        }

        public static string jinxinpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_jinxinpay_url"].ToString(); }
        }
        public static string jinxinpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jinxinpay_notifyUrl"].ToString(); }
        }
        public static string jinxinpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jinxinpay_callbackurl"].ToString(); }
        }

        public static string jinxinpay_logFile { get { return "jinxinpay.log"; } }

        #endregion

        #region 訊科支付

        public static string xkpay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_xkpay_partner"].ToString(); }
        }
        public static string xkpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xkpay_Key"].ToString(); }
        }

        public static string xkpay_wechatPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xkwechat_url"].ToString(); }
        }
        public static string xkpay_alipayPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xkalipay_url"].ToString(); }
        }
        public static string xkpay_bankPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xkbank_url"].ToString(); }
        }
        public static string xkpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xkpay_notifyUrl"].ToString(); }
        }
        public static string xkpay_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xkpay_callbackurl"].ToString(); }
        }


        public static string xkpay_logFile { get { return "xkpay.log"; } }

        #endregion

        #region 盛世支付

        public static string shengshi_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_shengshi_memberid"].ToString(); }
        }

        public static string shengshi_Key
        {
            get { return ConfigurationManager.AppSettings["pay_shengshi_Key"].ToString(); }
        }

        public static string shengshi_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_shengshi_url"].ToString(); }
        }
        public static string shengshi_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_shengshi_notifyUrl"].ToString(); }
        }
        public static string shengshi_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_shengshi_callbackurl"].ToString(); }
        }

        public static string shengshi_logFile { get { return "shengshi.log"; } }

        #endregion

        #region 火星支付

        public static string bitexpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_bitexpay_memberid"].ToString(); }
        }

        public static string bitexpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_bitexpay_Key"].ToString(); }
        }
        public static string bitexpay_Secret
        {
            get { return ConfigurationManager.AppSettings["pay_bitexpay_Secret"].ToString(); }
        }

        public static string bitexpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_bitexpay_url"].ToString(); }
        }
        public static string bitexpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_bitexpay_notifyUrl"].ToString(); }
        }
       

        public static string bitexpay_logFile { get { return "bitexpay.log"; } }

        #endregion

        #region 东方闪付支付

        public static string quickpay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_quickpay_partner"].ToString(); }
        }
        public static string quickpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_quickpay_Key"].ToString(); }
        }

        public static string quickpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_quickpay_url"].ToString(); }
        }
        public static string quickpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_quickpay_notifyUrl"].ToString(); }
        }
        public static string quickpay_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_quickpay_callbackurl"].ToString(); }
        }


        public static string quickpay_logFile { get { return "quickpay.log"; } }

        #endregion

        #region 新全付支付

        public static string xinquanfu_partner
        {
            get { return ConfigurationManager.AppSettings["pay_xinquanfu_partner"].ToString(); }
        }
        public static string xinquanfu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xinquanfu_Key"].ToString(); }
        }

        public static string xinquanfu_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xinquanfu_url"].ToString(); }
        }
        public static string xinquanfu_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xinquanfu_notifyUrl"].ToString(); }
        }
        public static string xinquanfu_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xinquanfu_callbackurl"].ToString(); }
        }


        public static string xinquanfu_logFile { get { return "xinquanfu.log"; } }

        #endregion

        #region 融海支付

        public static string ronghai_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_ronghai_memberid"].ToString(); }
        }

        public static string ronghai_Key
        {
            get { return ConfigurationManager.AppSettings["pay_ronghai_Key"].ToString(); }
        }

        public static string ronghai_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_ronghai_url"].ToString(); }
        }
        public static string ronghai_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ronghai_notifyUrl"].ToString(); }
        }
        public static string ronghai_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_ronghai_callbackurl"].ToString(); }
        }

        public static string ronghai_logFile { get { return "ronghai.log"; } }

        #endregion

        #region 68支付

        public static string liuba_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_liuba_memberid"].ToString(); }
        }

        public static string liuba_Key
        {
            get { return ConfigurationManager.AppSettings["pay_liuba_Key"].ToString(); }
        }

        public static string liuba_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_liuba_url"].ToString(); }
        }
        public static string liuba_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_liuba_notifyUrl"].ToString(); }
        }
        public static string liuba_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_liuba_callbackurl"].ToString(); }
        }

        public static string liuba_logFile { get { return "liuba.log"; } }

        #endregion

        #region 天猫支付

        public static string tmallpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_tmallpay_memberid"].ToString(); }
        }

        public static string tmallpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_tmallpay_Key"].ToString(); }
        }

        public static string tmallpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_tmallpay_url"].ToString(); }
        }
        public static string tmallpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_tmallpay_notifyUrl"].ToString(); }
        }
        public static string tmallpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_tmallpay_callbackurl"].ToString(); }
        }

        public static string tmallpay_logFile { get { return "tmallpay.log"; } }

        #endregion

        #region 小美支付

        public static string xiaomei_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_xiaomei_memberid"].ToString(); }
        }

        public static string xiaomei_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xiaomei_Key"].ToString(); }
        }

        public static string xiaomei_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xiaomei_url"].ToString(); }
        }
        public static string xiaomei_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xiaomei_notifyUrl"].ToString(); }
        }
        public static string xiaomei_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xiaomei_callbackurl"].ToString(); }
        }

        public static string xiaomei_logFile { get { return "xiaomei.log"; } }

        #endregion

        #region 澄支付

        public static string sumipay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_sumipay_memberid"].ToString(); }
        }

        public static string sumipay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_sumipay_Key"].ToString(); }
        }

        public static string sumipay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_sumipay_url"].ToString(); }
        }
        public static string sumipay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_sumipay_notifyUrl"].ToString(); }
        }
        public static string sumipay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_sumipay_callbackurl"].ToString(); }
        }

        public static string sumipay_logFile { get { return "sumipay.log"; } }

        #endregion

        #region 星支付
        public static string starpay_partnerid
        {
            get { return ConfigurationManager.AppSettings["pay_starpay_partnerid"].ToString(); }
        }
        public static string starpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_starpay_key"].ToString(); }
        }
        public static string starpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_starpay_url"].ToString(); }
        }
        public static string starpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_starpay_notifyUrl"].ToString(); }
        }
        public static string starpay_logFile { get { return "starpay.log"; } }

        #endregion

        #region 天翔支付

        public static string fytxpay_code
        {
            get { return ConfigurationManager.AppSettings["pay_fytxpay_code"].ToString(); }
        }

        public static string fytxpay_key
        {
            get { return ConfigurationManager.AppSettings["pay_fytxpay_key"].ToString(); }
        }

        public static string fytxpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_fytxpay_url"].ToString(); }
        }
        public static string fytxpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_fytxpay_notifyUrl"].ToString(); }
        }
        public static string fytxpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_fytxpay_callbackurl"].ToString(); }
        }

        public static string fytxpay_logFile { get { return "fytxpay.log"; } }

        #endregion

        #region 聚光支付

        public static string juguangpay_code
        {
            get { return ConfigurationManager.AppSettings["pay_juguangpay_code"].ToString(); }
        }

        public static string juguangpay_key
        {
            get { return ConfigurationManager.AppSettings["pay_juguangpay_key"].ToString(); }
        }

        public static string juguangpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_juguangpay_url"].ToString(); }
        }
        public static string juguangpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_juguangpay_notifyUrl"].ToString(); }
        }
        public static string juguangpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_juguangpay_callbackurl"].ToString(); }
        }

        public static string juguangpay_logFile { get { return "juguangpay.log"; } }

        #endregion

        #region 众诚支付

        public static string zhongchengpay_code
        {
            get { return ConfigurationManager.AppSettings["pay_zhongchengpay_code"].ToString(); }
        }

        public static string zhongchengpay_key
        {
            get { return ConfigurationManager.AppSettings["pay_zhongchengpay_key"].ToString(); }
        }

        public static string zhongchengpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_zhongchengpay_url"].ToString(); }
        }
        public static string zhongchengpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhongchengpay_notifyUrl"].ToString(); }
        }
        public static string zhongchengpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhongchengpay_callbackurl"].ToString(); }
        }

        public static string zhongchengpay_logFile { get { return "zhongchengpay.log"; } }

        #endregion

        #region 东方支付

        public static string dongfangpay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_dongfangpay_partner"].ToString(); }
        }

        public static string dongfangpay_key
        {
            get { return ConfigurationManager.AppSettings["pay_dongfangpay_key"].ToString(); }
        }

        public static string dongfangpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_dongfangpay_url"].ToString(); }
        }
        public static string dongfangpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_dongfangpay_notifyUrl"].ToString(); }
        }
        public static string dongfangpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_dongfangpay_callbackurl"].ToString(); }
        }

        public static string dongfangpay_logFile { get { return "dongfangpay.log"; } }

        #endregion

        #region 多宝支付

        public static string duobaopay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_duobaopay_partner"].ToString(); }
        }
        public static string duobaopay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_duobaopay_key"].ToString(); }
        }

        public static string duobaopay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_duobaopay_url"].ToString(); }
        }
        public static string duobaopay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_duobaopay_notifyUrl"].ToString(); }
        }
        public static string duobaopay_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_duobaopay_callbackurl"].ToString(); }
        }


        public static string duobaopay_logFile { get { return "duobaopay.log"; } }

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

        #region 金发支付

        public static string jfpay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_jfpay_partner"].ToString(); }
        }
        public static string jfpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_jfpay_Key"].ToString(); }
        }

        public static string jfpay_wechatPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_jfwechat_url"].ToString(); }
        }
        public static string jfpay_alipayPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_jfalipay_url"].ToString(); }
        }
        public static string jfpay_bankPayURL
        {
            get { return ConfigurationManager.AppSettings["pay_jfbank_url"].ToString(); }
        }
        public static string jfpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jfpay_notifyUrl"].ToString(); }
        }
        public static string jfpay_shopCallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jfpay_callbackurl"].ToString(); }
        }
        
        public static string jfpay_logFile { get { return "jfpay.log"; } }

        #endregion

        #region 诚易玖支付

        public static string chengyijiu_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_chengyijiu_memberid"].ToString(); }
        }

        public static string chengyijiu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_chengyijiu_Key"].ToString(); }
        }

        public static string chengyijiu_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_chengyijiu_url"].ToString(); }
        }
        public static string chengyijiu_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_chengyijiu_notifyUrl"].ToString(); }
        }
        public static string chengyijiu_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_chengyijiu_callbackurl"].ToString(); }
        }

        public static string chengyijiu_logFile { get { return "chengyijiu.log"; } }

        #endregion

        #region 完美支付

        public static string perfectpay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_perfectpay_memberid"].ToString(); }
        }

        public static string perfectpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_perfectpay_Key"].ToString(); }
        }

        public static string perfectpay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_perfectpay_url"].ToString(); }
        }
        public static string perfectpay_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_perfectpay_notifyUrl"].ToString(); }
        }
        public static string perfectpay_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_perfectpay_callbackurl"].ToString(); }
        }

        public static string perfectpay_logFile { get { return "perfectpay.log"; } }

        #endregion

        #region 收米支付

        public static string shoumipay_partner
        {
            get { return ConfigurationManager.AppSettings["pay_shoumipay_partner"].ToString(); }
        }
        public static string shoumipay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_shoumipay_Key"].ToString(); }
        }

        public static string shoumiwechatwap_url
        {
            get { return ConfigurationManager.AppSettings["pay_shoumiwechatwap_url"].ToString(); }
        }
        public static string shoumiwechatscan_url
        {
            get { return ConfigurationManager.AppSettings["pay_shoumiwechatscan_url"].ToString(); }
        }
        public static string shoumialipaywap_url
        {
            get { return ConfigurationManager.AppSettings["pay_shoumialipaywap_url"].ToString(); }
        }
        public static string shoumialipayscan_url
        {
            get { return ConfigurationManager.AppSettings["pay_shoumialipayscan_url"].ToString(); }
        }
        public static string shoumitoken_url
        {
            get { return ConfigurationManager.AppSettings["pay_shoumitoken_url"].ToString(); }
        }
        public static string shoumipay_notifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_shoumipay_notifyUrl"].ToString(); }
        }
        public static string shoumipay_callbackurl
        {
            get { return ConfigurationManager.AppSettings["pay_shoumipay_callbackurl"].ToString(); }
        }

        public static string shoumipay_logFile { get { return "shoumipay.log"; } }

        #endregion

        #region 轰达冬瓜支付

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

        #region 巅峰冬瓜支付

        public static string DFXiamipay_URL
        {
            get { return ConfigurationManager.AppSettings["pay_DFXiamipay_url"].ToString(); }
        }

        public static string DFXiamipay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_DFXiamipay_callbackurl"].ToString(); }
        }

        public static string DFXiamipay_appKey
        {
            get { return ConfigurationManager.AppSettings["pay_DFXiamipay_appKey"].ToString(); }
        }

        public static string DFXiamipay_appSecret
        {
            get { return ConfigurationManager.AppSettings["pay_DFXiamipay_appSecret"].ToString(); }
        }

        public static string DFXiamipay_logFile { get { return "DFXiamipay.log"; } }

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

        #region 河馬代付

        public static string Hema_MD5KEY { get { return ConfigurationManager.AppSettings["pay_hema_MD5KEY"].ToString(); } }

        public static string Hema_priKey { get { return ConfigurationManager.AppSettings["pay_hema_priKey"].ToString(); } }

        #endregion

        #region 永旺

        public static string Yongwang_ID
        {
            get { return ConfigurationManager.AppSettings["pay_yongwang_ID"].ToString(); }
        }

        public static string Yongwang_Key1
        {
            get { return ConfigurationManager.AppSettings["pay_yongwang_Key1"].ToString(); }
        }

        public static string Yongwang_Key2
        {
            get { return ConfigurationManager.AppSettings["pay_yongwang_Key2"].ToString(); }
        }

        public static string Yongwang_Url
        {
            get { return ConfigurationManager.AppSettings["pay_yongwang_url"].ToString(); }
        }

        public static string Yongwang_notifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_yongwang_notifyUrl"].ToString(); }
        }

        public static string Yongwang_logFile { get { return "Yongwang.log"; } }

        #endregion

        #region 佰富

        public static string Baifu_Url
        {
            get { return ConfigurationManager.AppSettings["pay_baifu_url"].ToString(); }
        }

        public static string Baifu_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_baifu_notifyurl"].ToString(); }
        }

        public static string Baifu_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_baifu_callbackurl"].ToString(); }
        }

        public static string Baifu_MerchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_baifu_merchantNo"].ToString(); }
        }

        public static string Baifu_Key
        {
            get { return ConfigurationManager.AppSettings["pay_baifu_key"].ToString(); }
        }

        public static string Baifu_logFile { get { return "Baifu.log"; } }

        #endregion

        #region 旗胜

        public static string QiSheng_Url
        {
            get { return ConfigurationManager.AppSettings["pay_qisheng_url"].ToString(); }
        }


        public static string QiSheng_Channelid
        {
            get { return ConfigurationManager.AppSettings["pay_qisheng_channelid"].ToString(); }
        }

        public static string QiSheng_Key
        {
            get { return ConfigurationManager.AppSettings["pay_qisheng_key"].ToString(); }
        }

        public static string QiSheng_logFile { get { return "QiSheng.log"; } }

        #endregion

        #region OK付

        public static string OKPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_okpay_url"].ToString(); }
        }


        public static string OKPay_MerchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_okpay_merchantNo"].ToString(); }
        }

        public static string OKPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_okpay_key"].ToString(); }
        }

        public static string OKPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_okpay_notifyurl"].ToString(); }
        }

        public static string OKPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_okpay_callbackurl"].ToString(); }
        }

        public static string OKPay_logFile { get { return "OKPay.log"; } }

        #endregion

        #region BDPAY

        public static string BDPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_bdpay_url"].ToString(); }
        }


        public static string BDPay_MerchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_bdpay_merchantNo"].ToString(); }
        }

        public static string BDPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_bdpay_key"].ToString(); }
        }

        public static string BDPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_bdpay_notifyurl"].ToString(); }
        }

        public static string BDPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_bdpay_callbackurl"].ToString(); }
        }

        public static string BDPay_logFile { get { return "BDPay.log"; } }

        #endregion

        #region 聚宝支付

        public static string jbpay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_jbpay_url"].ToString(); }
        }

        public static string jbpay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jbpay_notifyurl"].ToString(); }
        }
        public static string jbpay_MerchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_jbpay_merchantNo"].ToString(); }
        }

        public static string jbpay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_jbpay_key"].ToString(); }
        }

        public static string jbpay_logFile { get { return "jbpay.log"; } }

        #endregion

        #region 畅支付

        public static string Zumire_mchID
        {
            get { return ConfigurationManager.AppSettings["pay_zumire_mchid"].ToString(); }
        }

        public static string Zumire_Key
        {
            get { return ConfigurationManager.AppSettings["pay_zumire_Key"].ToString(); }
        }
     

        public static string Zumire_Url
        {
            get { return ConfigurationManager.AppSettings["pay_zumire_url"].ToString(); }
        }

        public static string Zumire_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zumire_notifyUrl"].ToString(); }
        }

        public static string ZumireWithdraw_Key
        {
            get { return ConfigurationManager.AppSettings["pay_zumire_WithdrawKey"].ToString(); }
        }

        public static string Zumire_logFile { get { return "Zumire.log"; } }

        #endregion

        #region 顺畅代付       

        public static string ShunchangWithdraw_Key
        {
            get { return ConfigurationManager.AppSettings["pay_shunchang_Key"].ToString(); }
        }

        public static string Shunchang_logFile { get { return "Shunchang.log"; } }

        #endregion

        #region 翔优支付

        public static string Xiangyou_Appid
        {
            get { return ConfigurationManager.AppSettings["pay_xiangyou_appid"].ToString(); }
        }

        public static string Xiangyou_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xiangyou_key"].ToString(); }
        }


        public static string Xiangyou_Url
        {
            get { return ConfigurationManager.AppSettings["pay_xiangyou_url"].ToString(); }
        }

        public static string Xiangyou_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xiangyou_notifyUrl"].ToString(); }
        }

        public static string Xiangyou_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xiangyou_callbackurl"].ToString(); }
        }

        public static string Xiangyou_logFile { get { return "Xiangyou.log"; } }

        #endregion
        
        #region 葡萄支付

        public static string Putao_merId
        {
            get { return ConfigurationManager.AppSettings["pay_putao_merId"].ToString(); }
        }

        public static string Putao_Key
        {
            get { return ConfigurationManager.AppSettings["pay_putao_Key"].ToString(); }
        }


        public static string Putao_Url
        {
            get { return ConfigurationManager.AppSettings["pay_putao_url"].ToString(); }
        }

        public static string Putao_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_putao_notifyUrl"].ToString(); }
        }

        public static string Putao_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_putao_callbackurl"].ToString(); }
        }

        public static string Putao_logFile { get { return "Putao.log"; } }

        #endregion
        
        #region 中邦支付

        public static string Zhongbang_mchId
        {
            get { return ConfigurationManager.AppSettings["pay_zhongbang_mchid"].ToString(); }
        }

        public static string Zhongbang_Key
        {
            get { return ConfigurationManager.AppSettings["pay_zhongbang_Key"].ToString(); }
        }

        public static string Zhongbang_Url
        {
            get { return ConfigurationManager.AppSettings["pay_zhongbang_url"].ToString(); }
        }

        public static string Zhongbang_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhongbang_notifyUrl"].ToString(); }
        }

        public static string Zhongbang_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhongbang_callbackurl"].ToString(); }
        }

        public static string Zhongbang_logFile { get { return "Zhongbang.log"; } }

        #endregion

        #region 星坤支付(RH-74, YH-76, XK-77)

        public static string XingKun_Merchant_No
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_merchant_no"].ToString(); }
        }

        public static string XingKun_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_Key"].ToString(); }
        }

        public static string XingKun_RHPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_rhpay_url"].ToString(); }
        }

        public static string XingKun_YHPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_yhpay_url"].ToString(); }
        }

        public static string XingKun_XKPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_xkpay_url"].ToString(); }
        }
        
        public static string XingKun_RHPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_rhpay_notifyUrl"].ToString(); }
        }

        public static string XingKun_YHPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_yhpay_notifyUrl"].ToString(); }
        }

        public static string XingKun_XKPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_xkpay_notifyUrl"].ToString(); }
        }

        public static string XingKun_ReturnUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingkun_returnUrl"].ToString(); }
        }

        public static string XingKun_RHPay_logFile { get { return "XingKunRH.log"; } }
        public static string XingKun_YHPay_logFile { get { return "XingKunYH.log"; } }
        public static string XingKun_XKPay_logFile { get { return "XingKunXK.log"; } }

        #endregion

        #region 高盛支付

        public static string Gaosheng_AppID
        {
            get { return ConfigurationManager.AppSettings["pay_gaosheng_appId"].ToString(); }
        }

        public static string GaoSheng_Url
        {
            get { return ConfigurationManager.AppSettings["pay_gaosheng_url"].ToString(); }
        }

        public static string GaoSheng_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_gaosheng_notifyUrl"].ToString(); }
        }

        public static string GaoSheng_ReturnUrl
        {
            get { return ConfigurationManager.AppSettings["pay_gaosheng_returnUrl"].ToString(); }
        }

        public static string GaoSheng_Key
        {
            get { return ConfigurationManager.AppSettings["pay_gaosheng_key"].ToString(); }
        }
        
        public static string GaoSheng_logFile { get { return "GaoSheng.log"; } }

        #endregion
        
        #region 一号代付

        public static string Yihao_MD5KEY { get { return ConfigurationManager.AppSettings["pay_yihao_MD5KEY"].ToString(); } }

        public static string Yihao_logFile { get { return "Yihao.log"; } }


        #endregion

        #region 蚂蚁代付

        public static string Mayi_Key { get { return ConfigurationManager.AppSettings["pay_mayi_key"].ToString(); } }

        public static string Mayi_logFile { get { return "Mayi.log"; } }


        #endregion

        #region 招财宝

        public static string ZhaoCaiBao_MerID
        {
            get { return ConfigurationManager.AppSettings["pay_zhaocaibao_merId"].ToString(); }
        }

        public static string ZhaoCaiBao_Url
        {
            get { return ConfigurationManager.AppSettings["pay_zhaocaibao_url"].ToString(); }
        }

        public static string ZhaoCaiBao_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhaocaibao_notifyUrl"].ToString(); }
        }

        public static string ZhaoCaiBao_ReturnUrl
        {
            get { return ConfigurationManager.AppSettings["pay_zhaocaibao_returnUrl"].ToString(); }
        }

        public static string ZhaoCaiBao_Key
        {
            get { return ConfigurationManager.AppSettings["pay_zhaocaibao_key"].ToString(); }
        }

        public static string ZhaoCaiBao_logFile { get { return "ZhaoCaiBao.log"; } }

        #endregion

        #region 贝富支付

        public static string BeifuPay_uname
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_uname"].ToString(); }
        }

        public static string BeifuPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_url"].ToString(); }
        }

        public static string BeifuPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_notifyUrl"].ToString(); }
        }

        public static string BeifuPay_sUrl
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_surl"].ToString(); }
        }

        public static string BeifuPay_DesKey
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_deskey"].ToString(); }
        }

        public static string BeifuPay_MD5Key
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_md5key"].ToString(); }
        }

        public static string BeifuPay_SysPwd
        {
            get { return ConfigurationManager.AppSettings["pay_beifupay_syspwd"].ToString(); }
        }

        public static string BeifuPay_LogFile { get { return "BeifuPay.log"; } }

        #endregion

        #region 实时支付

        public static string LivePay_MemberID
        {
            get { return ConfigurationManager.AppSettings["pay_livepay_memberid"].ToString(); }
        }

        public static string LivePay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_livepay_key"].ToString(); }
        }

        public static string LivePay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_livepay_url"].ToString(); }
        }

        public static string LivePay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_livepay_notifyUrl"].ToString(); }
        }

        public static string LivePay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_livepay_callbackurl"].ToString(); }
        }

        public static string LivePay_logFile { get { return "LivePay.log"; } }

        #endregion

        #region 唯创支付

        public static string WeiChuang_AppID
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_appid"].ToString(); }
        }

        public static string WeiChuang_Key
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_key"].ToString(); }
        }

        public static string WeiChuang_Url
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_url"].ToString(); }
        }

        public static string WeiChuang_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_notifyUrl"].ToString(); }
        }

        public static string WeiChuang_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_callbackurl"].ToString(); }
        }

        public static string WeiChuang_PKID
        {
            get { return ConfigurationManager.AppSettings["pay_WeiChuang_pkid"].ToString(); }
        }

        public static string WeiChuang_logFile { get { return "WeiChuang.log"; } }

        #endregion

        #region 金钻支付

        public static string JZPay_MemberID
        {
            get { return ConfigurationManager.AppSettings["pay_jzpay_memberid"].ToString(); }
        }

        public static string JZPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_jzpay_key"].ToString(); }
        }

        public static string JZPay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_jzpay_url"].ToString(); }
        }

        public static string JZPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jzpay_notifyUrl"].ToString(); }
        }

        public static string JZPay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_jzpay_callbackurl"].ToString(); }
        }

        public static string JZPay_logFile { get { return "JZPay.log"; } }

        #endregion

        #region 大栋代付

        public static string Dadong_Key { get { return ConfigurationManager.AppSettings["pay_dadong_key"].ToString(); } }

        public static string Xiaodong_Key { get { return ConfigurationManager.AppSettings["pay_xiaodong_key"].ToString(); } }

        public static string Dadong_logFile { get { return "Dadong.log"; } }


        #endregion

        #region 星耀支付

        public static string Xingyao_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_xingyao_memberid"].ToString(); }
        }

        public static string Xingyao_Key
        {
            get { return ConfigurationManager.AppSettings["pay_xingyao_Key"].ToString(); }
        }

        public static string Xingyao_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_xingyao_url"].ToString(); }
        }
        public static string Xingyao_shopNotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingyao_notifyUrl"].ToString(); }
        }
        public static string Xingyao_shopCallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_xingyao_callbackurl"].ToString(); }
        }

        public static string Xingyao_logFile { get { return "Xingyao.log"; } }

        #endregion

        #region 春秋支付

        public static string Chunqiu_appId
        {
            get { return ConfigurationManager.AppSettings["pay_chunqiu_appId"].ToString(); }
        }

        public static string Chunqiu_key
        {
            get { return ConfigurationManager.AppSettings["pay_chunqiu_key"].ToString(); }
        }

        public static string Chunqiu_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_chunqiu_url"].ToString(); }
        }

        public static string Chunqiu_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_chunqiu_notifyUrl"].ToString(); }
        }

        public static string Chunqiu_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_chunqiu_callbackurl"].ToString(); }
        }

        public static string Chunqiu_logFile { get { return "Chunqiu.log"; } }

        #endregion

        #region 旺旺宝

        public static string Wangwangbao_Url
        {
            get { return ConfigurationManager.AppSettings["pay_wangwangbao_url"].ToString(); }
        }


        public static string Wangwangbao_MerchantNo
        {
            get { return ConfigurationManager.AppSettings["pay_wangwangbao_merID"].ToString(); }
        }

        public static string Wangwangbao_Key
        {
            get { return ConfigurationManager.AppSettings["pay_wangwangbao_key"].ToString(); }
        }

        public static string Wangwangbao_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_wangwangbao_notifyUrl"].ToString(); }
        }

        public static string Wangwangbao_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_wangwangbao_callbackurl"].ToString(); }
        }

        public static string Wangwangbao_logFile { get { return "Wangwangbao.log"; } }

        #endregion

        #region 易收支付

        public static string Easypay_Url
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_url"].ToString(); }
        }

        public static string Easypay_AuthId
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_authId"].ToString(); }
        }        

        public static string Easypay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_notifyUrl"].ToString(); }
        }

        public static string Easypay_CallbackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_callbackurl"].ToString(); }
        }

        public static string Easypay_RSA_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_RSA_PublicKey"].ToString(); }
        }

        public static string Easypay_RSA_PrivateKey
        {
            get { return ConfigurationManager.AppSettings["pay_easypay_RSA_PrivateKey"].ToString(); }
        }

        public static string Easypay_logFile { get { return "Easypay.log"; } }

        #endregion

        #region 易联支付

        public static string Yilian_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_yilian_memberid"].ToString(); }
        }

        public static string Yilian_Key
        {
            get { return ConfigurationManager.AppSettings["pay_yilian_Key"].ToString(); }
        }

        public static string Yilian_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_yilian_url"].ToString(); }
        }
        public static string Yilian_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_yilian_notifyUrl"].ToString(); }
        }
        public static string Yilian_CallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_yilian_callbackurl"].ToString(); }
        }

        public static string Yilian_logFile { get { return "Yilian.log"; } }
        #endregion

        #region 点点支付

        public static string Diandian_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_diandian_memberid"].ToString(); }
        }

        public static string Diandian_Key
        {
            get { return ConfigurationManager.AppSettings["pay_diandian_Key"].ToString(); }
        }

        public static string Diandian_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_diandian_url"].ToString(); }
        }
        public static string Diandian_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_diandian_notifyUrl"].ToString(); }
        }
        public static string Diandian_CallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_diandian_callbackurl"].ToString(); }
        }

        public static string Diandian_logFile { get { return "Diandian.log"; } }

        #endregion

        #region opay支付

        public static string OPay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_opay_memberid"].ToString(); }
        }

        public static string OPay_Key
        {
            get { return ConfigurationManager.AppSettings["pay_opay_Key"].ToString(); }
        }

        public static string OPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_opay_url"].ToString(); }
        }
        public static string OPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_opay_notifyUrl"].ToString(); }
        }
        public static string OPay_CallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_opay_callbackurl"].ToString(); }
        }

        public static string OPay_logFile { get { return "OPay.log"; } }

        #endregion

        #region 怪物支付

        public static string MonsterPay_memberid
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_memberid"].ToString(); }
        }

        public static string MonsterPay_PrivateKey
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_PrivateKey"].ToString(); }
        }
        public static string MonsterPay_PublicKey
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_PublicKey"].ToString(); }
        }

        public static string MonsterPay_PayURL
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_url"].ToString(); }
        }
        public static string MonsterPay_NotifyUrl
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_notifyUrl"].ToString(); }
        }
        public static string MonsterPay_CallBackUrl
        {
            get { return ConfigurationManager.AppSettings["pay_monsterpay_callbackurl"].ToString(); }
        }

        public static string MonsterPay_logFile { get { return "MonsterPay.log"; } }

        #endregion

        /// <summary>
        /// 来源类型
        /// </summary>
        public enum PayState : byte
        {
            Success = 1,
            Error = 2,
            SignError = 3
        }

    }


}