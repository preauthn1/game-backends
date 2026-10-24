using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web
{
    public static class ModelStateExtensions
    {
        public static string GetValidateErrorMessage(this ModelStateDictionary modelState, string joinChar = "\\r\\n")
        {
            StringBuilder sbMessage = new StringBuilder();
            foreach (var err in modelState)
            {
                err.Value.Errors.ToList().ForEach(r => {
                    sbMessage.AppendFormat("{0}{1}", r.ErrorMessage, joinChar);
                });
            }

            return sbMessage.ToString();
        }
    }
}
