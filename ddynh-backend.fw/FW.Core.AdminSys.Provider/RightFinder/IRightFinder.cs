using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.RightFinder
{
    public interface IRightFinder
    {
        List<RightInfo> GetRightInfo();
    }
}
