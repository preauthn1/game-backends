using Microsoft.AspNetCore.Mvc;
using target.Utility;
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

    public class SuperAdminController : BaseController
    {
        public SuperAdminService SuperAdminService = new SuperAdminService();

        public TokenContractService TokenContractService = new TokenContractService();

        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/slots-game/change-token-contract-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> ChangeTokenContractAddressInSlotsContract([FromBody]LazyCommonRequestModel model)
        {
            model.TargetContractConfig = ContractConfig.SlotsLogicContract;
            var result = SuperAdminService.ChangeTokenContractAddressInSlotsContract(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/dragonTiger/change-token-contract-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> ChangeTokenContractAddressInDragonTigerContract([FromBody] LazyCommonRequestModel model)
        {
            model.TargetContractConfig = ContractConfig.DragonTigerLogicContract;
            var result = SuperAdminService.ChangeTokenContractAddressInSlotsContract(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/slots-game/change-nft-contract-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> ChangeNFTContractAddressInSlotsContract([FromBody] LazyCommonRequestModel model)
        {
            model.TargetContractConfig = ContractConfig.SlotsLogicContract;
            var result = SuperAdminService.ChangeNFTContractAddressInSlotsContract(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/dragonTiger/change-nft-contract-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> ChangeNFTContractAddressInDragonTiger([FromBody] LazyCommonRequestModel model)
        {
            model.TargetContractConfig = ContractConfig.DragonTigerLogicContract;
            var result = SuperAdminService.ChangeNFTContractAddressInSlotsContract(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/bet-contract/add-trust-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> AddAddressToTrustList([FromBody] LazyCommonRequestModel model)
        {
            var address = GetCurrentUserAccount();
            var result = SuperAdminService.AddAddressToTrustList(model);
            return ConvertToStandardFormat(result);
        }


        [PlayerTokenAuthorizeFilter]
        [Route("superAdmin/mint-balance-for-address")]
        [HttpPost]
        public ApiResult<CommonTransactionHashResultViewModel> MintBalanceForTargetAddress([FromBody] LazyCommonRequestModel model)
        {
            var result = TokenContractService.MintBalanceForTargetAddress(model.Address);
            return ConvertToStandardFormat(result);
        }



        //[Route("superAdmin/set-")]
        //[HttpPost]
        //public ApiResult<CommonTransactionHashResultViewModel> MintBalanceForTargetAddress([FromBody] LazyCommonRequestModel model)
        //{
        //    var result = TokenContractService.MintBalanceForTargetAddress(model.Address);
        //    return ConvertToStandardFormat(result);
        //}


    }
}
