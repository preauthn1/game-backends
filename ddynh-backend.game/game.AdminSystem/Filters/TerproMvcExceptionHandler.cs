using game.Entity;
using System;
using System.Web.Mvc;
using game.Infrastructure.Extensions;
using game.Infrastructure.Utility;
using FW.Core.Helper;

namespace game.AdminSystem.Filters
{
    /// <summary>
    /// 自訂的TeproException攔截處理
    /// </summary>
    public class TerproMvcExceptionHandler : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {         
            Exception e = filterContext.Exception;

            var terproException = e as TerproException;

            //如果是我方的terproException
            if (terproException != null)
            {
                var msg = terproException.ErrroCode.GetDescription();

                if(terproException.ErrroCode.ToString().Contains("InvalidParameter"))
                {
                    string column = terproException.ErrorDetail?.ColumnName;
                    if (!string.IsNullOrWhiteSpace(column))
                    {
                        msg += $" ({column})";
                    }
                }

                var isAjax = filterContext.RequestContext.HttpContext.Request.IsAjaxRequest();

                //如果是ajax，格式是json
                if(isAjax)
                {
                    string retrunUrl = filterContext.RequestContext.HttpContext.Request.UrlReferrer.ToString();
                    retrunUrl = retrunUrl + (retrunUrl.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();

                    filterContext.ExceptionHandled = true;
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new CRUDResultModel
                        {
                             IsSuccess = false,
                             Message = msg
                        }
                    };
                }
                else
                {
                    string retrunUrl = filterContext.RequestContext.HttpContext.Request.UrlReferrer.ToString();
                    retrunUrl = retrunUrl + (retrunUrl.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();

                    filterContext.ExceptionHandled = true;
                    filterContext.Result = new ContentResult
                    {
                        Content = $"<script>alert('{msg}');location.href='{retrunUrl}';</script>"
                    };
                }

                return;
            }
        }
    }

}