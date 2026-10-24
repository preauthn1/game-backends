using System;


namespace FW.Core.Auth.Token
{
    public interface IRightTokenBuilder
    {
        IRightToken CreateRightToken(string cmd);
    }
}
