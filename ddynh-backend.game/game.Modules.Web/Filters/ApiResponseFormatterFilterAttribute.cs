using game.Entity.ViewModel.GamePlayAPI.Core;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid.Http;
using System.Web;
using System.Web.Http.Filters;

namespace game.GamePlayAPI
{
    public class ApiResponseFormatterFilterAttribute : ActionFilterAttribute
    {
        //特例的url (不返回標準格式)
        private static List<string> _specialUrl = new List<string>
        {
            "",
            "/"
        };


        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            // 若發生例外則不在這邊處理
            if (actionExecutedContext.Exception != null)
                return;

            object result = null;

            bool isSpecialUrl = _specialUrl.Contains(System.Web.HttpContext.Current.Request.Url.AbsolutePath);
            
            if (actionExecutedContext.ActionContext.Response.Content != null)
            {
                // 取得由 API 返回的資料
                dynamic resultObject = actionExecutedContext.ActionContext.Response.Content.ReadAsAsync<object>().Result;

                result = new ApiResponseModel
                {
                    Data = resultObject
                };                
            }
            else
            {
                result = new ApiResponseModel();
            }
                        
            // 重新封裝回傳格式
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(result);
            base.OnActionExecuted(actionExecutedContext);
        }

    }
}