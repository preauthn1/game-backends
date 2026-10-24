using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace game.Infrastructure.Encrypt
{
    public class RSAHelper
    {
        #region 签名

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="key"></param>
        /// <param name="signString"></param>
        /// <returns></returns>
        public static string RSASign(string key, string signString)
        {
            try
            {
                key = RSAPrivateKeyJava2DotNet(key);
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                byte[] signBytes = rsa.SignData(UTF8Encoding.UTF8.GetBytes(signString), new MD5CryptoServiceProvider());
                return Convert.ToBase64String(signBytes);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 密钥效验签名
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="data">原始数据</param>
        /// <param name="signature">需要验签密文</param>
        /// <param name="cryptoType">加密算法</param>
        /// <returns></returns>
        public static bool VerifySign(string key, string data, string signature)
        {
            try
            {
                key = RSAPublicKeyJava2DotNet(key);
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(key);
                return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(data), new MD5CryptoServiceProvider(), Convert.FromBase64String(signature));
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        /// <summary>
        /// //RSA私钥格式转换
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
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

        /// <summary>
        /// RSA公钥格式转换
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        #region 加密
        /// <summary>
        ///  RSA 加密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="encryptString">被加密字符串</param>
        /// <param name="type">密钥转换类型</param>
        /// <returns></returns>
        public static string RSAEncrypt(string key, string encryptString)
        {
            key = RSAPrivateKeyJava2DotNet(key);
            string result;
            byte[] plainTextBArray;
            byte[] cypherTextBArray;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            plainTextBArray = (new UnicodeEncoding()).GetBytes(encryptString);
            cypherTextBArray = rsa.Encrypt(plainTextBArray, false);
            result = Convert.ToBase64String(cypherTextBArray);
            return result;
        }
        /// <summary>
        ///  RSA 加密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="encryptByte">被加密字节数组</param>
        /// <param name="type">密钥转换类型</param>
        /// <returns></returns>
        public static string RSAEncrypt(string key, byte[] encryptByte)
        {
            key = RSAPublicKeyJava2DotNet(key);
            string result;
            byte[] cypherTextBArray;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            cypherTextBArray = rsa.Encrypt(encryptByte, false);
            result = Convert.ToBase64String(cypherTextBArray);
            return result;
        }


        #endregion

        #region 解密
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static string RSADecrypt(string key, string decryptString)
        {
            key = RSAPrivateKeyJava2DotNet(key);
            byte[] plainTextBArray;
            byte[] dypherTextBArray;
            string result;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            plainTextBArray = Convert.FromBase64String(decryptString);
            dypherTextBArray = rsa.Decrypt(plainTextBArray, false);
            result = (new UnicodeEncoding()).GetString(dypherTextBArray);
            return result;
        }
        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="decryptByte">待解密的字节数组</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static string RSADecrypt(string key, byte[] decryptByte)
        {
            key = RSAPublicKeyJava2DotNet(key);
            byte[] dypherTextBArray;
            string result;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            dypherTextBArray = rsa.Decrypt(decryptByte, false);
            result = (new UnicodeEncoding()).GetString(dypherTextBArray);
            return result;
        }
        #endregion
    }
}
