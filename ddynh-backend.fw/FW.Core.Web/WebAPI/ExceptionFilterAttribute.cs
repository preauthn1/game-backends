using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace FW.Core.Web.WebAPI
{
    public class WebAPIExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            Logger.LogManager.GetLogger("exception").LogError(actionExecutedContext.Exception.ToString());
        }
    }
}
