using Microsoft.AspNetCore.Mvc;
using target.Models.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Utility.Extensions;
using target.Utility.Helper.Dependency;
using target.Models.Table;
using target.Utility;
using target.Utility.IOC;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Helper;

namespace target.Web.Modules.Filter
{
    public class ModalValidCustomizedResponse
    {

        public static BadRequestObjectResult ResponseCustomizedErrorResponse(ActionContext context)
        {
            var invalidColumns = context.ModelState.Where(x => x.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid);

            string method = context.HttpContext.Request.Method;
            string msg = $"Invalid parameter or format. 您所带入的表单请求参数格式不正确 ({method})";

            if (invalidColumns.IsNotEmptyNull())
            {
                string myErrorMsg = "Invalid parameter or format. 非合法格式，请检查您填写的参数";
              
                if (method == "POST")
                {
                    var bodyPayload = context.HttpContext.Request.GetRequestBodyString();

                    if (string.IsNullOrWhiteSpace(bodyPayload))
                    {
                        myErrorMsg = "Error! HttpPost payload is empty. 您的POST请求表单为空值 (发送POST 请勿把参数放在querystring)";
                    }
                }

                if(invalidColumns.Count() == 1 && invalidColumns.FirstOrDefault().Key.ContainsWihtoutCase("playerid"))
                {
                    var allColumns = string.Join("、", invalidColumns.Select(z => z.Key));

                    var codeErrorMsg = MultiLanguageTextHelper.GetErrorCodeMsg(ApiErrorCode.InvalidParameter_IsNotValidPlayerIdFormat , null);

                    msg = $"{codeErrorMsg} ";
                }
                else
                {
                    var allColumns = string.Join("、", invalidColumns.Select(z => z.Key));
                    msg = $"{allColumns} {myErrorMsg}  ({method})";
                }
            }
            else
            {
                var error = context.ModelState.Root.Errors.FirstOrDefault();
                if(error.ErrorMessage.Contains("non-empty"))
                {
                    string extraMsg = string.Empty;
                    if(context.HttpContext.Request.Method == "GET")
                    {
                        extraMsg = "（检查您代码中是否正确标记 [FromQuery]？）";
                    }

                    msg = $"不允許请求参数為空。{extraMsg}";
                }
                else
                {
                    msg = msg + error.ErrorMessage;
                }
            }

            var res = new ApiResponseErrorModel
            {
                Code = ApiErrorCode.InvalidParameter_ParsingError.ToInteger(),
                Message = msg,
                Data = null
            };
            
            var logModel = new ApiRequestLogEntity();
            HttpRequestLogHelper.SetRequestInfo(context.HttpContext.Request, logModel);
            DataModelConvertHelper.SetDebugInfoBeforeWriteLog(logModel, res);
            logModel.ResponseStatusCode = 400;

            //errorLog紀錄
            string tableName = SettingHelper.ApiRequestLogTableName_Error;
            var service = IOCHelper.Resolve<IApiRequestLogService>();
            service.CreateTryCatch(tableName, logModel);
            
            return new BadRequestObjectResult(res)
            {
                ContentTypes = { "application/json" }
            };
        }

    }
}
