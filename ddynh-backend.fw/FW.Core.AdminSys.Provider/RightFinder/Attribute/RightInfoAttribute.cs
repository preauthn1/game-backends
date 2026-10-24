using FW.Core.AdminSys.Provider.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.RightFinder.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RightInfoAttribute : System.Attribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="rightName"></param>
        /// <param name="rightType"></param>
        /// <param name="rightToken"></param>
        /// <param name="sort"></param>
        /// <param name="pageUrl"></param>
        public RightInfoAttribute(string rightName, RightType rightType, string rightToken, int sort = 0, string pageUrl = "")
        {
            RightName = rightName;
            RightType = rightType;
            RightToken = [REDACTED];
            Sort = sort;
            PageUrl = pageUrl;
        }

        public string RightName
        {
            get;
            private set;
        }

        public RightType RightType
        {
            get;
            private set;
        }

        public string RightToken
        {
            get;
            private set;
        }

        public string PageUrl
        {
            get;
            private set;
        }

        public int Sort
        {
            get;
            private set;
        }

    }
}
