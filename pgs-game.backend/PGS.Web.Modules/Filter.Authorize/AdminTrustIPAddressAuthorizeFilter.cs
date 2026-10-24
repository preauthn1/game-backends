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
using target.Models.Enums;
using System.Collections.Generic;

namespace target.Web.Modules.Filter
{

    public class AdminTrustIPAddressAuthorizeFilter : Attribute, IAuthorizationFilter , IOrderedFilter
    {

        public RestrictLevel Level { get; set; }


        public int Order { get; set; } = 500;



        public AdminTrustIPAddressAuthorizeFilter()
        {
            this.Level = RestrictLevel.MaxRestrictLevel;
        }

        public AdminTrustIPAddressAuthorizeFilter(RestrictLevel targetLevel)
        {
            this.Level = targetLevel;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var myIP = IPHelper.GetIP();

            var service = IOCHelper.Resolve<IAdminSysValidateCheckLogicRefService>();

            string jwt = context.HttpContext.Request.GetHeaderValueByKey(SystemConstant.HeaderKey_AccessToken);
            AdminUserTokenJwtModel userJwtModel = MyJwtHelper.GetAdminUserModelFromJwtString(jwt);

            string myUserName = userJwtModel?.LoginName ?? "";

            //沒userName = 未登入。(此檢查邏輯不要拿掉)
            if(string.IsNullOrWhiteSpace(myUserName))
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            var checkResult = service.CheckIPAddressAndSourceIsValid(myIP, myUserName, this.Level);

            if (checkResult.IsValidateSuccess)
                return;

            string extraMsg = "";

            var errorCode = ApiErrorCode.Forbidden_WatchDataPermissionNotAllow;
            string extraHintInfo = $"**403｜{myIP}";


            bool isEmi_or_joker = myIP.StartsWith("193.36.23") || myUserName.ContainsWihtoutCase("Emi");

            if (isEmi_or_joker ||
               myUserName.ContainsWihtoutCase("Admin-") ||
               myUserName.ContainsWihtoutCase("target-"))
            {
                errorCode = ApiErrorCode.Forbidden_WatchDataPermissionNotAllow_CompanyIP;

                if(isEmi_or_joker)
                {
                    extraHintInfo += "｜💀部分功能僅允許公司IP下操作";
                }
            }            

            throw new TerproException(errorCode, new ErrorDetailModel
            {
                ColumnName = extraHintInfo,
                IsAlwaysNotify = true,
                LogExtraInfo = $"{myIP} , {myUserName}",
            });

        }
    }




}
