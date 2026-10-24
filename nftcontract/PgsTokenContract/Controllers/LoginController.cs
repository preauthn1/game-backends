using Microsoft.AspNetCore.Mvc;
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
   
    public class LoginController : BaseController
    {
        public LoginService LoginService = new LoginService();

        public TokenContractService ContractService = new TokenContractService();



        [Route("login/nonce-gen")]
        [HttpPost]
        public ApiResult<LoginNonceViewModel> GenerateNonceMessage([FromBody] LazyCommonRequestModel model)
        {
            var result = LoginService.GenerateNonceMessage(model);
            return ConvertToStandardFormat(result);
        }


        [Route("login")]
        [HttpPost]
        public ApiResult<LoginResultViewModel> Login([FromBody]LoginRequestModel model)
        {
            var result = LoginService.Login(model);

            //首次綁定 自動送xx元 (pgsERC20Token)
            ContractService.BindAddressAndSendFirstTimeBonus(model.Address);

            return ConvertToStandardFormat(result);
        }



        /// <summary>
        /// 測試環境debug用
        /// </summary>
        [Route("dev-debug/login/nonce-gen")]
        [HttpGet]
        public ApiResult<object> Debug_GenerateNonceMessage([FromQuery]SignValidateDebugRequestModel model)
        {
            object result = LoginService.GenerateNonceMessage_ForDevDebug(model);
            return ConvertToStandardFormat(result);
        }


        /// <summary>
        /// 測試環境debug用
        /// </summary>
        [Route("dev-debug/login/validate-sign")]
        [HttpPost]
        public ApiResult<object> Debug_TestSignIsValid([FromBody] SignValidateDebugRequestModel model)
        {
            object result = LoginService.ValidateSignCheck_ForDevDebug(model);
            return ConvertToStandardFormat(result);
        }


    }
}
