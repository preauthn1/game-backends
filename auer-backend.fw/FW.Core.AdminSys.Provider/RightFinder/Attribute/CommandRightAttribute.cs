using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.RightFinder.Attribute
{
    public class CommandRightAttribute : RightInfoAttribute
    {
        public CommandRightAttribute(string rightName, string rightToken = "", int sort = 0)
            :base(rightName, Entity.RightType.CommandRight, rightToken, sort, "")
        { }

    }
}
