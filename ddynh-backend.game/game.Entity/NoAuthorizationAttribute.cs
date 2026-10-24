using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace game.Entity
{
    public class NoAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            System.Web.HttpContext.Current.Items.Add("SkipAuth", "true");
        }

    }
}
