using System;


namespace FW.Core.Auth.Token
{
    public class StringRightTokenBuilder : IRightTokenBuilder
    {

        public IRightToken CreateRightToken(string cmd)
        {
            return new Token.StringRightToken(cmd);
        }

    }
}
