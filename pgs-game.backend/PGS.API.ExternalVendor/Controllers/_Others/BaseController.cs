using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Helper;
using target.Utility.IOC;
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

namespace target.API.ExternalVendor.Controllers
{
    [ApiController]
    [Consumes("application/json" , "application/x-www-form-urlencoded")]
    [Produces("application/json" , "application/x-www-form-urlencoded")]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
            InstanceAllServiceImplement(this);
        }


        protected void InstanceAllServiceImplement<T>(T controller) where T : class
        {
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsInterface)
                .Where(x => x.Name.ToLower().Contains("service"))
                .Where(x => !x.Name.ToLower().Contains("integrationservice")) //遊戲商service不採用自動注入
                .ToList();

            foreach (var item in allService)
            {
                //特定命名的service 不自動注入
                if (item.Name == "GameVendorIntegrationService")
                    continue;

                var instanceValue = IOCHelper.Resolve(item.PropertyType);
                item.SetValue(this, instanceValue);
            }
        }



        /// <summary>
        /// 根據配置 模擬API錯誤發生 (建議新對接一家遊戲商，要測試下對方有無retry機致)
        /// </summary>
        protected void Develop_Simulate_ExceptionOnApiBeginging(GameVendorTypeEnum gameVendor, SystemConfigSettingEnum type, string referOrderNo = "")
        {
            if (SettingHelper.IsProduction)
                return;

            var rep = new SystemConfigSettingRepository();

            var queryTargetGameVendor = rep.GetParseModelByCache(SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor);

            //檢查指定的是哪一個遊戲商
            if (queryTargetGameVendor.SettingValueOfInt != (int)gameVendor)
                return;

            var query = rep.GetParseModelByCache(type);

            if (query == null)
                return;

            var requiredValue = query.SettingValueOfInt;

            int rndValue = RandomHelper.GetRandIntegerNumbers(1, 99);

            if (requiredValue > rndValue)
            {
                // 需要驗證下對方是前一次拿到失敗回應才下一次
                Thread.Sleep(1000 * 2);

                if (!string.IsNullOrWhiteSpace(referOrderNo))
                {
                    HeaderValueHelper.AppendReferOrderNo(referOrderNo);
                }

                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SimulateErrorOccur);
            }

            return;
        }

        /// <summary>
        /// 根據配置 模擬延遲 (建議新對接一家遊戲商，要測試下對方有無retry機致)
        /// </summary>
        protected void Develop_Simulate_DelayTimeout(GameVendorTypeEnum gameVendor, SystemConfigSettingEnum type)
        {
            if (SettingHelper.IsProduction)
                return;

            var rep = new SystemConfigSettingRepository();

            var queryTargetGameVendor = rep.GetParseModelByCache(SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor);

            //檢查指定的是哪一個遊戲商
            if (queryTargetGameVendor.SettingValueOfInt != (int)gameVendor)
                return;

            var query = rep.GetParseModelByCache(type);

            if (query == null)
                return;

            var delaySeconds = query.SettingValueOfInt;

            if (delaySeconds <= 0)
                return;

            Thread.Sleep(1000 * delaySeconds);
            return;
        }









    }
}
