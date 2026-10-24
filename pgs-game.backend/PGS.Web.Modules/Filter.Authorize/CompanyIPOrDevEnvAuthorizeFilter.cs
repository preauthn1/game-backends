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

    public class CompanyIPOrDevEnvAuthorizeFilter : Attribute, IAuthorizationFilter
    {

        public CompanyIPOrDevEnvAuthorizeFilter()
        {

        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var myIP = IPHelper.GetIP();

            if (SettingHelper.IsDev)
                return;

            if (IPHelper.IsCompanyIP(myIP))
                return;

            if (myIP == "::ffff:0.0.0.0")
            {
                var password = [REDACTED]"password"];                     

                if(password == SystemConstant.PublicAPI_SignKey_MagicPassword)
                {
                    return;
                }                
            }               

            throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"{myIP}",
            });
        }
    }




}
