using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Cache
{
    public class DepencyRegister : SystemDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            engine.Register<ICacheManager, InProcCacheManager>();
        }
    }
}
