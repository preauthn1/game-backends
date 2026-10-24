using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;
using target.Utility.Helper;
using System;
using target.Utility;
using target.Utility.IOC;
using target.BL.Interface;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace target.Web.Modules.Filter
{

    public class PostHashHeaderValidateFilter : Attribute, IActionFilter , IOrderedFilter
    {

        public int Order { get; set; } = 5;


        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext != null)
            {
                var myBody = context.HttpContext.Request.GetRequestBodyString();

                if (myBody == null)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_Body);
                }

                string postHash = HeaderValueHelper.GetPostHashHeader();

                var errorDetail = new ErrorDetailModel
                {
                    ColumnName = "post body",
                    IsAlwaysNotify = true,
                };

                if (string.IsNullOrWhiteSpace(postHash))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, errorDetail);
                }

                var keyValues = myBody.JsonConvertToModel<Dictionary<string, object>>();

                string hashText = string.Join("&", keyValues
                    .OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => $"{x.Key}={ConvertHashValue(x.Value)}"))
                    .ToLowerInvariant();

                string finalHash = (hashText + "3d1a6d06c246").MD5Hash();

                if (finalHash != postHash)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter, errorDetail);
                }
            }

            return;
        }


        private string ConvertHashValue(object value)
        {
            if (value == null)
                return "null";

            if (value is JArray array)
            {
                if (array.All(x => x.Type == JTokenType.Object))
                {
                    return "[" + string.Join(",", array.Select(x => x.ConvertToJsonString())) + "]";
                }
                else
                {
                    return "[" + string.Join(",", array.Select(x => x.ToString().Trim())) + "]";
                }
            }
            if (value is JObject myObj)
                return string.Join(",", myObj.ConvertToJsonString());

            if (value is DateTime time)
                return string.Join(",", time.ToISO8601());

            return value.ToString();
        }


        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }


    }

}
