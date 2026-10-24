using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;

namespace game.GameAPI.Controllers
{
    public class DepencyRegister : ApplicationDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            var types = this.GetType().Assembly.GetTypes();
            types.ToList().ForEach(t =>
            {
                if (typeof(IHttpController).IsAssignableFrom(t))
                {
                    engine.Register(t, LifeStyle.Transient);
                }
            });
        }

    }
}