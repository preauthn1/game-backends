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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.IOC;

namespace target.API.ExternalVendor.Controllers
{
    [Consumes("text/xml")]
    [Produces("text/xml")]
    [ApiController]
    public abstract class BaseGameVendorControllerXML : ControllerBase
    {
        public BaseGameVendorControllerXML()
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


        public IGameVendorSingleWalletService SingleWalletVendorService { get; set; }

        protected abstract GameVendorTypeEnum _gameVendor { get; }



        /// <summary>
        /// 根據配置 模擬API錯誤發生 (建議新對接一家遊戲商，要測試下對方有無retry機致)
        /// </summary>
        protected void Develop_Simulate_ExceptionOnApiBeginging(GameVendorTypeEnum gameVendor , SystemConfigSettingEnum type)
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
