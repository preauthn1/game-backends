using Microsoft.AspNetCore.Mvc;
using target.Models;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using PgsTokenContract.Models;
using PgsTokenContract.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace target.API.AdminSys.Controllers
{
    [Consumes("application/json")]
    [Produces("application/json")]
    [ApiController]
    public class BaseController : ControllerBase
    {

        public PlayerJwtCheckService PlayerJwtCheckService = new PlayerJwtCheckService();

        protected ApiResult<T> ConvertToStandardFormat<T>(T model)
        {
            if (!ContractConfig.IsSystemEnable)
                throw new TerproException(ApiErrorCode.SystemMaintain);

            return new ApiResult<T>
            {
                Code = 0,
                Message = "OK",
                Data = model
            };
        }


        protected string GetCurrentUserAccount()
        {
            string jwtToken = [REDACTED];

            var parsing = PlayerJwtCheckService.CheckAndParsingJwt(jwtToken);

            return parsing.LoginName;
        }

    }
}
