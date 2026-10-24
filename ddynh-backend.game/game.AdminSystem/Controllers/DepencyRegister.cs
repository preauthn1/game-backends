using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers
{
    public class DepencyRegister : ApplicationDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            var types = this.GetType().Assembly.GetTypes();
            Array.ForEach(types, t =>
            {
                if (typeof(IController).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                {
                    engine.Register(t, LifeStyle.Transient);
                }
            });
        }
    }
}