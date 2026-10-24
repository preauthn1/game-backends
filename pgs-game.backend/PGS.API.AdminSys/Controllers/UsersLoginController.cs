using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using System.Threading;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using target.Models.Models;

namespace target.API.AdminSys.Controllers
{
    public class UsersLoginController : BaseController
    {
        public IUserLoginService UserLoginService { get; set; }


        [Route("users/login")]
        [HttpPost]
        public UserLoginViewModel Login(UserLoginRequestModel model)
        {
            var query = UserLoginService.Login(model);
            return query;
        }


        [AdminTokenrAuthorizeFilter]
        [Route("users/bind-2FA")]
        [HttpPost]
        public void Bind2FA(UserLoginRequestModel model)
        {
            int userID = GetCurrentUserID();
            UserLoginService.FinishBindF2A(userID, model.Auth2FACode);
            return;
        }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("common/server-time")]
        [HttpPost]
        public object GenerateHashKeyByServerTime()
        {
            var currentTime = DateTimeHelper.GetCurrentTime();
            var hashResult = AppHelper.GenerateHashKeyByServerTime(currentTime);
            return new 
            {
                time = currentTime,
                hash = hashResult.Hash0
            };
        }

    }
}
