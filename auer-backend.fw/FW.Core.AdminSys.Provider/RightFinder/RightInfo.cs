using FW.Core.AdminSys.Provider.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.RightFinder
{
    public class RightInfo
    {
        /// <summary>
        /// ctor
        /// </summary>
        public RightInfo()
        {
            ChildRights = new List<RightInfo>();
        }

        public string RightName
        {
            get;
            set;
        }

        public string RightToken
        {
            get;
            set;
        }

        public string PageUrl
        {
            get;
            set;
        }

        public RightType RightType
        {
            get;
            set;
        }

        public int Sort
        {
            get;
            set;
        }

        public List<RightInfo> ChildRights
        {
            get;
            set;
        }

    }
}
