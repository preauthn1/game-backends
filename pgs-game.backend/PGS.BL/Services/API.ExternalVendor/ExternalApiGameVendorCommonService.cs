using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{
    public class ExternalApiGameVendorCommonService : BaseService, IExternalApiGameVendorCommonService
    {
        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }


        public bool CheckIsValidGameVendorIPAddress(string ip, GameVendorTypeEnum gameVendor , bool isThrowException = true)
        {
            if (SettingHelper.IsLocalhostWebSiteName && !SettingHelper.IsMSTest)
                return true;

            var config = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(gameVendor);

            if (config.ServerIPs.IsEmptyOrNull())
                return true;

            if (config.ServerIPs.Contains(ip))
                return true;

            string gameVendorID = gameVendor.ToInteger().ToString();

            //額外邏輯 從DB查 (如果已知是常態性IP 請加入程式碼中 減少DB存取
            var querySettings = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.GameVendorCallbackExtraWhiteListIP);

            var matchSetting = querySettings
                .Where(x => x.SubCategory == gameVendorID)
                .FirstOrDefault();

            if(matchSetting != null)
            {
                var availableIPs = new SystemConfigSettingParseModel(0, matchSetting.SettingValue).SettingValueOfListString;

                if(availableIPs.Any(i => i == ip))
                {
                    return true;
                }
            }

            if (isThrowException)
            {
                string logText = $"{gameVendor.ToString()} , {ip} ";
                throw new TerproException(ApiErrorCode.Forbidden_IPAddressNotAllow, new ErrorDetailModel
                {
                    ColumnName = logText,
                    IsAlwaysNotify = true,
                });
            }
            else
            {
                return false;
            }
        }

    }



}
