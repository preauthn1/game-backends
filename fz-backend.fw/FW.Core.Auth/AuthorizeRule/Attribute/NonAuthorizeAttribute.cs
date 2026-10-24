using System;


namespace FW.Core.Auth.AuthorizeRule.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=false)]
    public class NonAuthorizeAttribute : System.Attribute
    {

    }
}
