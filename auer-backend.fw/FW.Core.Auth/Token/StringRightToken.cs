using System;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Auth.Token
{
    /// <summary>
    /// 基于简单字符串定义的权限标识
    /// </summary>
    public class StringRightToken : [REDACTED]
    {
        private bool _matchAll = false;

        /// <summary>
        /// ctor
        /// </summary>
        public StringRightToken()
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="token"></param>
        public StringRightToken(string token)
        {
            if (token == "*")
            {
                _matchAll = true;
                return;
            }

            Token = [REDACTED] char[]{'/'}).ToList();
        }

        public List<string> Token
        {
            get;
            set;
        }

        public bool IsMatch(IRightToken token)
        {
            var stoken = [REDACTED] as StringRightToken;

            if (stoken == null)
                return false;

            if (_matchAll)
                return true;

            for (int i = 0; i < Token.Count; i++)
            {
                if (Token[i] != "*" && String.Compare(stoken.Token[i], Token[i], true) != 0)
                {
                    return false;
                }
            }

            return true;

        }

        public IRightToken ParseToken(string tokenStr)
        {
            return new StringRightToken(tokenStr);
        }
    }
}
