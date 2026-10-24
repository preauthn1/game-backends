using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class MerchantSelfUpdateSettingService : BaseAdminSysService, IMerchantSelfUpdateSettingService
    {
        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }



        public MerchantSelfUpdateSettingViewModel GetMerchantAvailableSelfUpdateSettings(int userID , int merchantID)
        {
            // 2023/11 已改用  [dbo].[MerchantGameSettingSelfConfig] 的方法管理
            return new MerchantSelfUpdateSettingViewModel
            {
                LobbyVisibleGames = new List<MerchantGameFlagSelfSettingViewModel>(),
                MerchantID = merchantID
            };

            //CheckViewMerchantPermission(userID, merchantID);

            //var queryMerchantGames = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
            //{
            //    MerchantIDs = merchantID.ToListCollection(),
            //    Status = NormalActiveStatus.Enable.ToInteger(),
            //    EnableGameOnly = true
            //});

            //var merchantGameExtraDisableSetting = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(merchantID, MerchantCustomizedSettingKeyEnum.MerchantSelfSetting_LobbyDisabledGameIDs, isReturnNull: false);

            //List<int> disableGameIDs = merchantGameExtraDisableSetting.SettingValueOfListInt;

            //var gameEnableViewModels = queryMerchantGames.Select(z => new MerchantGameFlagSelfSettingViewModel
            //    {
            //        GameID = z.GameID,
            //        GameName = DbMultiLanguageTextHelper.GetGameDisplayName(z.GameID),
            //        IsEnable = disableGameIDs.Contains(z.GameID) ? false : true
            //    })
            //    .ToList();

            //return new MerchantSelfUpdateSettingViewModel
            //{
            //    LobbyVisibleGames = gameEnableViewModels,
            //    MerchantID = merchantID
            //};
        }


        public void UpdateMerchantLobbyVisibleGames(int userID, MerchantSelfUpdateSettingViewModel model)
        {

            // 2023/11 已改用  [dbo].[MerchantGameSettingSelfConfig] 的方法管理
            return;

            //model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            //model.RuleFor(z => z.LobbyVisibleGames).Rule_NotEmptyNull();

            //CheckViewMerchantPermission(userID , model.MerchantID);

            //var disableGameIDs = model.LobbyVisibleGames
            //    .Where(z => !z.IsEnable)
            //    .Select(z => z.GameID)
            //    .ToList();

            //int merchantID = model.MerchantID;
            //int settingKey = MerchantCustomizedSettingKeyEnum.MerchantSelfSetting_LobbyDisabledGameIDs.ToInteger();

            //var existData = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            //{
            //    MerchantIDs = merchantID.ToListCollection(),
            //    CustomizedSettingKeyIDs = settingKey.ToListCollection()

            //}).FirstOrDefault();

            //string settingValue = string.Join(",", disableGameIDs);

            //if(existData == null)
            //{
            //    MerchantCustomizedSettingRepository.Create(new MerchantCustomizedSettingEntity
            //    {
            //        MerchantID = merchantID,
            //        CustomizedSettingKey = settingKey,
            //        CreateTime = DateTimeHelper.GetCurrentTime(),
            //        SettingValue = settingValue
            //    });
            //}
            //else
            //{
            //    existData.SettingValue = settingValue;
            //    MerchantCustomizedSettingRepository.Update(existData);
            //}

            ////更新redis
            //CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting(merchantID, MerchantCustomizedSettingKeyEnum.MerchantSelfSetting_LobbyDisabledGameIDs.ToInteger());
        
        
        }

    }
}
