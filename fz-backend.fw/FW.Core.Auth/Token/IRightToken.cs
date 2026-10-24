using System;


namespace FW.Core.Auth.Token
{
    public interface IRightToken
    {
        /// <summary>
        /// 是否匹配当前权限标识
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        bool IsMatch(IRightToken token);

        /// <summary>
        /// 解析权限标识
        /// </summary>
        /// <param name="tokenStr"></param>
        /// <returns></returns>
        IRightToken ParseToken(string tokenStr);
    }

  
}
