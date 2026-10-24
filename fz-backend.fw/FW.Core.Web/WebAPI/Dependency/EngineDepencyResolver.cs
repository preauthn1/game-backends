using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Web.Http.Dependencies;


namespace FW.Core.Web.WebAPI.Dependency
{
    public class EngineDepencyResolver : EngineWebApiScope, IDependencyResolver
    {

        public IDependencyScope BeginScope()
        {
            return new EngineDepencyResolver();
        }
    }
}
