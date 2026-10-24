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

    public class AddressController : BaseController
    {
        public TokenContractService ContractService = new TokenContractService();


        [PlayerTokenAuthorizeFilter]
        [Route("address/info")]
        [HttpGet]
        public ApiResult<BalanceResultViewModel> GetWalletInfoByAddress()
        {
            var address = GetCurrentUserAccount();
            var result = ContractService.GetBalance(address);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("address/bind")]
        [HttpGet, HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> BindAddressAndSendBonus()
        {
            var address = GetCurrentUserAccount();
            var result = ContractService.BindAddressAndSendFirstTimeBonus(address);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("address/bind-test-target")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> ManualTriggerBindAddressAndSendBonusToTargetAddress([FromBody]LazyCommonRequestModel model)
        {
            var result = ContractService.BindAddressAndSendFirstTimeBonus(model.Address);
            return ConvertToStandardFormat(result);
        }



        [Route("transaction/explore")]
        [HttpGet]
        public object GetTransactionResultByTxt([FromQuery] LazyCommonRequestModel model)
        {
            var result = ContractService.GetTransactionResultByTxHash(model.TransactionHash);
            return new
            {
                code = 0,
                msg = "Success",
                result = result
            };
        }



    }
}
