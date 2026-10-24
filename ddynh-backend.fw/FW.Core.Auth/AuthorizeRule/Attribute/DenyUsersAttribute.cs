using System;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Auth.AuthorizeRule.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple=true)]
    public class DenyUsersAttribute : System.Attribute
    {

        public List<string> DenyUsers
        {
            get;
            set;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="users"></param>
        public DenyUsersAttribute(params string[] users)
        {
            DenyUsers = users.ToList();
        }


    }
}
