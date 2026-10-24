using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.BL.Interface;
using target.Models;
using target.Utility.Helper;
using target.Utility.Helper.Dependency;
using target.Utility.IOC;

namespace target.API.Public.Controllers
{
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class BaseController : ControllerBase
    {

        public IPlayerJwtCheckService PlayerJwtCheckService { get; set; }


        public BaseController()
        {
            InstanceAllServiceImplement(this);
        }

        /// <summary>
        /// 若未登入或失敗會返回0
        /// </summary>
        /// <returns></returns>
        protected int TryGetLobbyTokenPlayerID()
        {
            try
            {
                return GetLobbyTokenPlayerID();
            }
            catch
            {

            }

            return 0;
        }

        protected int GetLobbyTokenPlayerID()
        {
            string token = [REDACTED];

            //因為改成guid的模式 一定要查DB 才能反查了
            var parseModel = PlayerJwtCheckService.ParsingLobbyToken(token);

            if (parseModel != null && parseModel.ParsingTokenModel != null)
            {
                HeaderValueHelper.AppendLogInfoUserIdentity(parseModel.ParsingTokenModel.LoginUserID);
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_IdentityMemo, parseModel.ParsingTokenModel.MerchantID.ToString());
            }

            return parseModel.ParsingTokenModel.LoginUserID;
        }


        protected void SetLanguageByLobbyToken()
        {
            string token = [REDACTED];

            //因為改成guid的模式 一定要查DB 才能反查了
            var parseModel = PlayerJwtCheckService.ParsingLobbyToken(token);

            if (parseModel != null)
            {
                string myLang = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_Lang);
                if (string.IsNullOrWhiteSpace(myLang))
                {
                    HeaderValueHelper.ForseSetHeaderLanguage(parseModel.ParsingTokenModel.Lang);
                }
            }
        }


        protected void InstanceAllServiceImplement<T>(T controller) where T : class
        {
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsInterface)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();

            foreach (var item in allService)
            {
                var instanceValue = IOCHelper.Resolve(item.PropertyType);
                item.SetValue(this, instanceValue);
            }
        }


    }
}
