using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc
{
    public class MvcExceptionFilterAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            Logger.LogManager.GetLogger("exception").LogError(filterContext.Exception.ToString());

            base.OnException(filterContext);
        }
    }
}
