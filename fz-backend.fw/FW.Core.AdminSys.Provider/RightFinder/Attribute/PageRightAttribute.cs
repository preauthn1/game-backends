using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.RightFinder.Attribute
{
    public class PageRightAttribute : RightInfoAttribute
    {
        public PageRightAttribute(string rightName, string pageUrl = "",  string rightToken = "", int sort = 0)
            :base(rightName, Entity.RightType.PageRight, rightToken, sort, pageUrl)
        { }
    }
}
