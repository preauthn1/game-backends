using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace FW.Core.Web.WebAPI
{
    public class NullableParameterBinding : HttpParameterBinding
    {
        public NullableParameterBinding(HttpParameterDescriptor parameter)
            :base(parameter)
        { }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpContextBase context = (HttpContextBase)actionContext.Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;

            string pname = Descriptor.ParameterName;
            string pvalue = request.QueryString[pname];
            if (String.IsNullOrWhiteSpace(pvalue))
            {
                pvalue = request.Headers[pname];
            }
            if (String.IsNullOrWhiteSpace(pvalue))
            {
                pvalue = request.Form[pname];
            }

            if (String.IsNullOrWhiteSpace(pvalue))
            {
                actionContext.ActionArguments[pname] = null;
            }
            else if (Descriptor.ParameterType == typeof(String))
            {
                actionContext.ActionArguments[pname] = pvalue;
            }
            else if (Descriptor.ParameterType.IsEnum)
            {
                actionContext.ActionArguments[pname] = Enum.Parse(Descriptor.ParameterType, pvalue, true);
            }
            else if (Descriptor.ParameterType.IsGenericType && Descriptor.ParameterType.GetGenericArguments()[0].IsEnum)
            {
                actionContext.ActionArguments[pname] = Enum.Parse(Descriptor.ParameterType.GetGenericArguments()[0], pvalue,true);
            }
            else
            {
                object ret = Convert.ChangeType(pvalue, Descriptor.ParameterType.GenericTypeArguments[0]);
                actionContext.ActionArguments[pname] = ret;
            }

            var tsc = new TaskCompletionSource<object>();
            tsc.SetResult(null);
            return tsc.Task;


        }


    }
}
