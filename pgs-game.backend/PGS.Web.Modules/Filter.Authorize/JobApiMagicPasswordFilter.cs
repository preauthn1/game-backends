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

namespace target.Web.Modules.Filter
{
   
    public class JobApiMagicPasswordFilter : Attribute , IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (SettingHelper.IsDev)
                return;

            if (IPHelper.IsCompanyIP())
                return;

            string password = [REDACTED]"Password"];

            if (password == SystemConstant.JobAPI_Login_MagicPassword)
                return;

            throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel
            {
                ColumnName = "JobAPI parameter"
            });
        }

    }

}
