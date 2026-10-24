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
using PgsTokenContract.Services;

namespace target.Web.Modules.Filter
{
   
    public class PlayerTokenAuthorizeFilter : Attribute , IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!ContractConfig.IsSystemEnable)
                throw new TerproException(ApiErrorCode.SystemMaintain);

            string jwtToken = [REDACTED];

            var service = new PlayerJwtCheckService();            
            service.CheckAndParsingJwt(jwtToken);

            //string jwtHash = jwtToken.MD5Hash();

            //var service = new PlayerJwtCheckService();

            ////反查DB 檢查token是否過期
            //var isExist = service.CheckIsValidToken(jwtHash);

            //if(!isExist)
            //    throw new TerproException(ApiErrorCode.Authorize_TokenHasChanged);


        }

    }

}
