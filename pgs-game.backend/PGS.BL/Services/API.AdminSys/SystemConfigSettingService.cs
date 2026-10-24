using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using target.Utility.Extensions;
using target.Models.Models;
using target.DA.Repository;
using target.Models.Enums;
using target.Utility.Helper;
using target.Models.ViewModels.API.AdminSys;
using target.Models.Attributes;
using Lck.Utility.Extensions.Helper;
using target.Utility;

namespace target.BL.Services
{
    public class SystemConfigSettingService : BaseService, ISystemConfigSettingService
    {

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public SystemConfigSettingPageViewModel GetAllSystemConfigSetting(int userID)
        {
            var queryAllSettings = SystemConfigSettingRepository.GetAll();

            var allEnums = ExtensionOfEnum.GetEnumAllValue<SystemConfigSettingEnum>().Select(x =>
                new SettingEnumStandardFormatConvertModel
                {
                    Group = x.GetAttributeFromEnum<DataCategoryAttribute>()?.Value ?? DataCategoryEnum.Common,
                    SettingKey = x.ToInteger(),
                    SettingKeyEnum = x
                })
                .ToList();

            var convertResult = SettingKeyEnumLogicHelper.ConvertToEditPageViewModel(allEnums);

            //正式環境隱藏特定的配置
            if(SettingHelper.IsProduction)
            {
                convertResult.Groups = convertResult.Groups
                    .Where(x => x.GroupID != (int)DataCategoryEnum.ErrorSimulate)
                    .ToList();
            }

            foreach(var item in convertResult.Groups.SelectMany(z=> z.Items))
            {
                var dbSetting = queryAllSettings.FirstOrDefault(x => x.ID == item.SettingKey);              

                if(dbSetting != null)
                {
                    //把db設值帶入
                    item.Value = dbSetting.SettingValue;
                }

                //特定的設置會限制下拉選單的值
                if (SettingKeyEnumLogicHelper.IsSpecificDataType<SystemMaintainStatusEnum>((SystemConfigSettingEnum)item.SettingKey))
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<SystemMaintainStatusEnum>();
                }
                else if (SettingKeyEnumLogicHelper.IsSpecificDataType<LobbyMainApiVersionEnum>((SystemConfigSettingEnum)item.SettingKey))
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<LobbyMainApiVersionEnum>();
                }
                else if (SettingKeyEnumLogicHelper.IsSpecificDataType<NormalYesOrNoEnum>((SystemConfigSettingEnum)item.SettingKey))
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<NormalYesOrNoEnum>();
                }
                else if (SettingKeyEnumLogicHelper.IsSpecificDataType<GameVendorTypeEnum>((SystemConfigSettingEnum)item.SettingKey))
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<GameVendorTypeEnum>();
                    item.DataDropdowns.Insert(0, new DropdownValueViewModel
                    {
                        Text = "None (不設定)",
                        Value = "0",
                        ValueInt = 0
                    });
                }
            }


            return convertResult;
        }


        public SystemConfigSettingParseModel GetSystemConfigSetting(SystemConfigSettingEnum settingKey)
        {
            var query = SystemConfigSettingRepository.GetParseModelByCache(settingKey);
            return query;
        }



        public void UpdateSystemSettings(int userID, SystemConfigSettingUpdateRequestModel model)
        {
            model.RuleFor(x => x.Items).Rule_NotEmptyNull();

            //檢查格式合法
            SettingKeyEnumLogicHelper.ValidateInputSettingValue<SystemConfigSettingEnum>(model.Items);

            foreach (var item in model.Items)
            {
                //迴圈編輯 (這功能不常使用 應該還好)
                var queryExist = SystemConfigSettingRepository.GetById(item.SettingKey);

                if (queryExist == null)
                {
                    SystemConfigSettingRepository.Create(new SystemConfigSettingEntity
                    {
                        ID = item.SettingKey,
                        SettingValue = item.Value
                    });
                }
                else
                {
                    queryExist.SettingValue = item.Value;
                    SystemConfigSettingRepository.Update(queryExist);
                }

                CacheRemoveHelper.RemoveCache_WhenUpdateSystemSetting(item.SettingKey);
            }
        }




    }

}
