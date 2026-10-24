using Microsoft.AspNetCore.Mvc;
using target.Web.Modules.Filter;
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
   
    public class BetController : BaseController
    {
        public BetService BetService = new BetService();


        [PlayerTokenAuthorizeFilter]
        [Route("bet/placeBet")]
        [HttpPost]
        public ApiResult<BetPlaceBetAndTransactionIDModel> PlaceBet(BetRequestModel model)
        {
            model.Address = GetCurrentUserAccount();
            var result = BetService.PlaceBet(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("bet/history/query")]
        [HttpGet]
        public ApiResult<BetResultModel> GetBetResult([FromQuery]LazyCommonRequestModel model)
        {
            model.Address = GetCurrentUserAccount();
            var result = BetService.GetBetResult(model);
            return ConvertToStandardFormat(result);
        }

    }
}
