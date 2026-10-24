using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace FW.Core.Web.WebAPI.Dependency
{
    public class EngineHttpControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
           var controller = (IHttpController)EngineContext.Current.Resolve(controllerType);

           return controller;
        }
    }
}
