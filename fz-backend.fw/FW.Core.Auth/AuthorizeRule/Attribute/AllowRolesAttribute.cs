using System;
using System.Linq;
using System.Collections.Generic;

namespace FW.Core.Auth.AuthorizeRule.Attribute
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple=true)]
    public class AllowRolesAttribute : System.Attribute
    {
        public List<String> AllowRoles
        {
            get;
            set;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="roles"></param>
        public AllowRolesAttribute(params string[] roles)
        {
            AllowRoles = roles.ToList();
        }
        
    }
}
