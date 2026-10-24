using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI
{
    public class DepencyRegister : SystemDepencyRegister
    {

        public override void Register(IEngine engine)
        {
            engine.Register<IAPIConsumerProvider, ConfigAPIConsumerProvider>();
        }

    }
}
