using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper.Dependency;

namespace target.BL.Services
{

    public partial class DebugUtilityService : BaseEventService, IDebugUtilityService
    {
       
        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }


        public SppedTestDebugInfo GetSeeedTestInfo(int playerID)
        {
            var queryItems = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.SpeedTestUrls.ToInteger(),                
            });

            var result = new SppedTestDebugInfo
            {

            };

            if(queryItems.IsNotEmptyNull())
            {
                result.Urls = queryItems
                    .Where(x => x.SettingValue.IsNotEmptyTextNotNull())
                    .Where(x => !IsImageKeyword(x.SettingValue))
                    .Select(x => new SppedTestApiUrlModel
                    {
                        DisplayName = x.Memo.IsEmptyOrNull() ? $"Url{x.SubCategory}" : x.Memo,
                        Url = x.SettingValue
                    })
                    .ToList(); 
                
                result.Images = queryItems
                     .Where(x => x.SettingValue.IsNotEmptyTextNotNull())
                    .Where(x => IsImageKeyword(x.SettingValue))
                     .Select(x => new SppedTestApiUrlModel
                     {
                         DisplayName = x.Memo.IsEmptyOrNull() ? $"Image{x.SubCategory}" : x.Memo,
                         Url = x.SettingValue
                     })
                     .ToList();
            }

            result.ServerTime_UTC8 = DateTimeHelper.GetCurrentTime().AddHours(8).ToString("yyyy/MM/dd HH:mm:ss");
            return result;
        }


        private bool IsImageKeyword(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return url.Contains(".png") || url.Contains(".jpg") || url.Contains(".jpeg");
        }

    }
}
