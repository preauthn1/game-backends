using System;


namespace FW.Core.Helper.Encrypt
{
    public static class EncryptHelper
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string MD5(string code)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(code, "md5");
        }

        /// <summary>
        /// Base64 编码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string Base64(string code)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(code));
        }

        /// <summary>
        /// 约定密码加密
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string PasswordEncrypt(string code)
        {
            return MD5(Base64(MD5(Base64(code))));
        }

    }
}
