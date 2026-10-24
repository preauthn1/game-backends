using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public class MerchantCustomizedSettingService : BaseAdminSysService, IMerchantCustomizedSettingService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public IBackendButtonsService BackendButtonsService { get; set; }


        public SystemConfigSettingPageViewModel GetMerchantAdvanceSettingsBySpecificKey(int userID, int merchantID, MerchantCustomizedSettingQueryParameter extraParam)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            CheckViewMerchantPermission(userID, merchantID);

            var querySettings = GetMerchantAdvanceSettings(userID, merchantID);

            //若有指定單筆settingKey
            if (extraParam != null && extraParam.SpecificKey.HasValue)
            {
                var groupViewModels = new List<SystemConfigSettingGroupViewModel>();

                //過濾掉其他的配置 只顯示參數指定的那筆
                foreach (var group in querySettings.Groups)
                {
                    var isMatch = group.Items.Any(z => z.SettingKey == extraParam.SpecificKey.Value);

                    if (isMatch)
                    {
                        group.Items = group.Items.Where(z => z.SettingKey == extraParam.SpecificKey.Value).ToList();
                        groupViewModels.Add(group);
                    }
                }
                querySettings.Groups = groupViewModels;
            }
            else if (extraParam != null && extraParam.SettingKeys.IsNotEmptyNull())
            {
                var groupViewModels = new List<SystemConfigSettingGroupViewModel>();

                //過濾掉其他的配置 只顯示參數指定的那筆
                foreach (var group in querySettings.Groups)
                {
                    var isMatch = group.Items.Any(z => extraParam.SettingKeys.Contains(z.SettingKey));

                    if (isMatch)
                    {
                        group.Items = group.Items.Where(z => extraParam.SettingKeys.Contains(z.SettingKey)).ToList();
                        groupViewModels.Add(group);
                    }
                }

                querySettings.Groups = groupViewModels
                    .OrderBy(x => x.GroupID == (int)DataCategoryEnum.SingleWalletSetting ? 0 : 999)
                    .ToList();
            }

            //防呆 避免致命錯誤
            if (!currentUser.IsSuperAdminOrg)
            {
                var totalItems = querySettings.Groups.SelectMany(x => x.Items).Count();

                if (totalItems > 20)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "#Q2001 Agent view data unauthorized.");
                }
            }

            var querySubAgentCount = MerchantHierarchyRepository.GetCountByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = merchantID
            });

            querySettings.NumberOfSubAgents = querySubAgentCount - 1;

            return querySettings;
        }



        public SystemConfigSettingPageViewModel GetMerchantAdvanceSettings(int userID, int merchantID)
        {
            //因為 GetAvailableMerchantCustomizedSettingsWhenCreateMerchant 的關係
            //請務必要開放帶merchantID=0 的狀況

            var queryMerchantSettings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            var allEnums = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>()
                .Select(x =>
                new SettingEnumStandardFormatConvertModel
                {
                    Group = x.GetAttributeFromEnum<DataCategoryAttribute>()?.Value ?? DataCategoryEnum.Common,
                    SettingKey = x.ToInteger(),
                    SettingKeyEnum = x
                })
                .ToList();

            var convertResult = SettingKeyEnumLogicHelper.ConvertToEditPageViewModel(allEnums);

            foreach (var item in convertResult.Groups.SelectMany(z => z.Items))
            {
                var dbSetting = queryMerchantSettings.FirstOrDefault(x => x.CustomizedSettingKey == item.SettingKey);

                if (dbSetting != null)
                {
                    //把db設值帶入
                    item.Value = dbSetting.SettingValue;
                }
                var settingKeyEnum = (MerchantCustomizedSettingKeyEnum)item.SettingKey;

                var dataType = settingKeyEnum.GetAttributeFromEnum<ReferType>();

                if (SettingKeyEnumLogicHelper.IsDataTypeYesOrNoEnum((MerchantCustomizedSettingKeyEnum)item.SettingKey))
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<NormalYesOrNoEnum>();
                }
                else if (dataType != null && dataType.Type == typeof(SystemTagEnum))
                {
                    var allEnumValues = ExtensionOfEnum.GetEnumAllValue<SystemTagEnum>();

                    var settingDefaultValue = settingKeyEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;

                    List<SystemTagEnum> requiredItems = new List<SystemTagEnum>();

                    if (!string.IsNullOrWhiteSpace(settingDefaultValue) && settingDefaultValue.IsNumeric())
                    {
                        SystemTagEnum defaultValue = (SystemTagEnum)Int32.Parse(settingDefaultValue);

                        DataCategoryEnum myCategory = defaultValue.GetAttributeFromEnum<DataCategoryAttribute>().Value;

                        requiredItems = allEnumValues
                            .Where(x => x.GetAttributeFromEnum<DataCategoryAttribute>().Value == myCategory)
                            .ToList();
                    }
                    else
                    {
                        requiredItems = new List<SystemTagEnum>();
                    }

                    item.DataDropdowns = requiredItems.Select(x =>
                    {
                        int intValue = x.ToInteger();
                        return new DropdownValueViewModel
                        {
                            Value = (intValue).ToString(),
                            ValueInt = intValue,
                            Text = $"{intValue}｜" + MultiLanguageTextHelper.GetEnumsCultureText(x)
                        };
                    })
                    .ToList();
                }
                else if (dataType != null && dataType.Type.IsEnum)
                {
                    item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType(dataType.Type);
                }
                else if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.AdminSys_CustomizedDropdownCategory)
                {
                    var queryItems = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
                    {
                        OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.AdminSys_MerchantDropdownCategory.ToInteger()
                    });

                    if (queryItems.IsNotEmptyNull())
                    {
                        item.DataDropdowns = queryItems.Select(x => new DropdownValueViewModel
                        {
                            Text = x.SettingValue,
                            Value = x.SubCategory,
                        })
                            .ToList();
                    }
                }
                else if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.Common_IMChannelID)
                {
                    var queryItems = IMChannelRepository.GetIMChannels();

                    if (queryItems.IsNotEmptyNull())
                    {
                        item.DataDropdowns = queryItems
                            .Select(x => new DropdownValueViewModel
                            {
                                Text = $"{x.IMChannelID}｜{MultiLanguageTextHelper.GetEnumsCultureText((IMChannelTypeEnum)x.Type)}｜{x.DisplayName}",
                                Value = x.IMChannelID.ToString(),
                                ValueInt = x.IMChannelID,
                            })
                            .ToList();
                    }
                }
                else if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting)
                {
                    var queryItems = IPAddressListTemplateRepository.GetByFilter(new QueryIPAddressListTemplateRequestViewModel
                    {

                    });

                    if (queryItems.IsNotEmptyNull())
                    {
                        item.DataDropdowns = queryItems
                            .Select(x => new DropdownValueViewModel
                            {
                                Text = $"{x.Id}｜{x.DisplayName}",
                                Value = x.Id.ToString(),
                            })
                            .ToList();
                    }
                }

                var targetSettingKey = (MerchantCustomizedSettingKeyEnum)item.SettingKey;

                item.KeywordForSearch =
                    (
                        targetSettingKey.ToString() +
                        MultiLanguageTextHelper.GetEnumsCultureText(targetSettingKey, LanguageEnum.en_us) +
                        MultiLanguageTextHelper.GetEnumsCultureText(targetSettingKey, LanguageEnum.zh_cn) +
                        MultiLanguageTextHelper.GetEnumsCultureText(targetSettingKey, LanguageEnum.zh_tw)
                    ).ToLower();
            }

            //自訂某些配置的順序
            convertResult.Groups = convertResult.Groups
                .OrderBy(z =>
                {
                    var groupID = z.GroupID;

                    if (groupID == DataCategoryEnum.Special_DisableEdit.ToInteger())
                        return 99999;

                    if (groupID == DataCategoryEnum.DevEnableOnly.ToInteger())
                        return 90000;

                    if (groupID == DataCategoryEnum.ConvertRate.ToInteger())
                        return 88888;

                    if (groupID == DataCategoryEnum.MerchantTag.ToInteger())
                        return 77777;

                    return groupID;
                })
                .ToList();

            foreach (var item in convertResult.Groups)
            {
                item.KeywordForSearch = string.Join("", item.Items.Select(x => x.KeywordForSearch));
            }

            convertResult.OwnerID = merchantID;
            convertResult.OwnerName = MerchantRepository.GetSingleByCache(merchantID)?.Name;
            convertResult.GroupCategoryList = convertResult.Groups.Select(x => new SystemConfigSettingGroupViewModel
            {
                GroupID = x.GroupID,
                GroupName = x.GroupName
            })
                .ToList();

            return convertResult;
        }


        public List<SystemConfigSettingViewModel> GetAvailableMerchantCustomizedSettingsWhenCreateMerchant(int userID)
        {
            var settings = GetMerchantAdvanceSettings(userID, 0);

            //定義新建商戶時 允許開放編輯的項目
            List<int> requiredKeys = new List<int>
            {
                MerchantCustomizedSettingKeyEnum.Common_IMChannelID.ToInteger(),
                MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl.ToInteger(),
                MerchantCustomizedSettingKeyEnum.System_IsMerchantCreditMode.ToInteger(),
            };

            //只有測試環境開放
            if (SettingHelper.IsDev)
            {
                requiredKeys.Add(MerchantCustomizedSettingKeyEnum.DefaultWalletBalance.ToInteger());
                requiredKeys.Add(MerchantCustomizedSettingKeyEnum.IsInternalTestMerchant.ToInteger());
            }

            var filterSettings = settings.Groups.SelectMany(z => z.Items)
                .Where(z => requiredKeys.Contains(z.SettingKey))
                .ToList();

            bool isBWG = SettingHelper.IsBWG;

            foreach (var item in filterSettings)
            {
                if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_IsUseIframeGameUrl)
                {
                    if (isBWG)
                    {
                        item.Value = "0"; //BWG預設關閉
                    }
                    else
                    {
                        item.Value = "1";
                    }
                }
            }

            return filterSettings;
        }



        public void UpdateSettings(int userID, SystemConfigSettingUpdateRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Items).Rule_NotEmptyNull();

            CheckViewMerchantPermission(userID, model.MerchantID);

            bool isAnyConvertRate = model.Items
                 .Select(z => ((MerchantCustomizedSettingKeyEnum)z.SettingKey).GetAttributeFromEnum<DataCategoryAttribute>())
                 .Any(x => x?.Value == DataCategoryEnum.ConvertRate);

            if (isAnyConvertRate)
            {
                CheckAllSystemMaintainOrException();
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                var validKeys = new List<int>
                {
                    MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel.ToInteger(),
                    MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain.ToInteger()
                };

                bool isValidSettingKey = model.Items.All(x => validKeys.Contains(x.SettingKey));

                if (!isValidSettingKey)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("invalid settings"));
                }
            }

            //檢查格式合法
            SettingKeyEnumLogicHelper.ValidateInputSettingValue<MerchantCustomizedSettingKeyEnum>(model.Items, model.MerchantID);

            string beforeValue = "";

            var updateEntities = new List<MerchantCustomizedSettingEntity>();

            foreach (var item in model.Items)
            {
                //部分項目跳過不支援手動編輯
                if (SettingKeyEnumLogicHelper.IsSkipManualEdit((MerchantCustomizedSettingKeyEnum)item.SettingKey))
                {
                    continue;
                }

                //迴圈編輯 (這功能不常使用 應該還好)
                var queryExist = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
                {
                    MerchantIDs = model.MerchantID.ToListCollection(),
                    CustomizedSettingKeyIDs = item.SettingKey.ToListCollection()
                })
                    .FirstOrDefault();

                if (queryExist != null)
                {
                    beforeValue = $"{queryExist.CustomizedSettingKey}:{queryExist.SettingValue}｜";
                }

                if (queryExist == null)
                {
                    var insertEntity = new MerchantCustomizedSettingEntity
                    {
                        CustomizedSettingKey = item.SettingKey,
                        MerchantID = model.MerchantID,
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        SettingValue = item.Value
                    };

                    MerchantCustomizedSettingRepository.Create(insertEntity);
                    updateEntities.Add(insertEntity);
                }
                else
                {
                    queryExist.SettingValue = item.Value;
                    MerchantCustomizedSettingRepository.Update(queryExist);
                    updateEntities.Add(queryExist);
                }

                CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting(model.MerchantID, item.SettingKey);
            }

            //額外API更新
            DoExtraAGGApiUpdate(updateEntities);

            string afterValue = string.Join("｜", model.Items.Select(x => $"{x.SettingKey}:{x.Value}"));

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateMerchantAdvanceSetting, model.MerchantID, new UserOperateChangeLogModel
            {
                BeforeObject = beforeValue,
                AfterObject = afterValue
            });

            log.Memo = string.Join(",", model.Items.Select(x => x.SettingKey));

            WriteOperateLog(log);

            CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting_WhenAllDone(model.MerchantID);

            if (isAnyConvertRate)
            {
                //強制登出所有當前玩家
                BackendButtonsService.ResetMerchantPlayerSessionToken(new BackendDefineButtonActionExtraParameterModel
                {
                    MerchantID = model.MerchantID
                });
            }
        }

        private void CheckAllSystemMaintainOrException()
        {
            var progressMaintain = VendorMaintenanceRepository.GetByFilter(new QueryVendorMaintenanceRequestViewModel
            {
                IsOnProgressing = true,
            });

            bool isAllSystemMaintain = progressMaintain.IsNotEmptyNull() && progressMaintain.Count() >= 2;

            if (!isAllSystemMaintain)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "此類型配置，僅允許維護期間(全遊戲商維護)調整。");
            }
        }

        public CRUDResultModel BatchUpdateMerchantAdvanceSettings(int userID, MerchantBatchUpdateSettingRequestModel model)
        {
            model.RuleFor(x => x.MerchantIDsRawText).Rule_NotEmptyText();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingValue).Rule_NotEmptyText(); //先不支援清除空字串

            if (model.SettingValue.ToLower() == "empty")
            {
                model.SettingValue = "";
            }

            var merchantIDs = AppHelper.AnalyzeIDsFromRawText(model.MerchantIDsRawText);

            if (merchantIDs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundMerchant);
            }

            List<MerchantCustomizedSettingEntity> insertItems = new List<MerchantCustomizedSettingEntity>();

            var now = DateTimeHelper.GetCurrentTime();

            foreach (var id in merchantIDs)
            {
                insertItems.Add(new MerchantCustomizedSettingEntity
                {
                    CustomizedSettingKey = model.SettingKey,
                    SettingValue = model.SettingValue,
                    MerchantID = id,
                    CreateTime = now
                });
            }

            MerchantCustomizedSettingRepository.MultipleDelete(merchantIDs, (MerchantCustomizedSettingKeyEnum)model.SettingKey);

            MerchantCustomizedSettingRepository.BulkInsert(insertItems);

            Task.Factory.StartNew(() =>
            {
                foreach (var item in insertItems)
                {
                    CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting(item.MerchantID, item.CustomizedSettingKey);
                    CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting_WhenAllDone(item.MerchantID);
                }
            });

            //基本上是針對AGG特殊配置更新 要同時透過API更新他們那邊的資料
            DoExtraAGGApiUpdate(insertItems);

            foreach (var mID in merchantIDs)
            {
                DoExtraAGGApiUpdate(mID, model.SettingKey, model.SettingValue);
            }

            string debugInfo = $"{model.ConvertToJsonString()}";

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.BatchUpdateMerchantCustomized, 0, debugInfo);
            WriteOperateLog(log);

            return new CRUDResultModel
            {
                Message = $"更新完成，共{merchantIDs.Count()}個商戶被更新",
            };
        }

        private void DoExtraAGGApiUpdate(int merchantID, int settingKey , string settingValue)
        {
            settingValue = settingValue ?? "";

            if (settingKey == (int)MerchantCustomizedSettingKeyEnum.GameSetting_EnableIDGFeature)
            {
                var aggService = new AGGIntegrationService();

                try
                {
                    aggService.ChangeLobbyEventEnableFlag(new Models.ViewModels.GameVendor.IDGLobbyEventChangeEnableFlagParameter
                    {
                        MerchantID = merchantID,
                        IsEnable = settingValue.Trim() == "1" ? true : false,
                    });
                }
                catch(Exception ex)
                {
                    string myMsg = $"商戶更新錯誤: {merchantID} , {ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(myMsg);
                }
            }
        }


        private void DoExtraAGGApiUpdate(List<MerchantCustomizedSettingEntity> settings)
        {
            //假如編輯的項目 分類屬於IDG
            var idgSettingKeys = ExtensionOfEnum.GetEnumAllValue<MerchantCustomizedSettingKeyEnum>()
                .Where(x => x.GetAttributeFromEnum<DataCategoryAttribute>()?.Value == DataCategoryEnum.GameSetting_IDG)
                .Select(x => x.ToInteger())
                .ToList();

            var idgSettings = settings
                .Where(x => idgSettingKeys.Contains(x.CustomizedSettingKey)).ToList();

            if (idgSettingKeys.IsEmptyOrNull())
                return;

            var groupByMerchant = idgSettings.GroupBy(x => x.MerchantID).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in groupByMerchant)
            {
                int merchantID = item.Key;
                var updateSettings = item.Value;

                var aggService = new AGGIntegrationService();

                try
                {
                    aggService.CreateAGGNamespaceByAPI(merchantID);
                }
                catch
                {

                }

                foreach (var eachSetting in updateSettings)
                {
                    //因為這個原因 暫時不實作後續
                    //因為這個原因 暫時不實作後續
                    //因為這個原因 暫時不實作後續

                    //會，該商戶會開啟活動，但是客端會噴錯。
                    //主要是服務器slot88 - vnd 沒有經過我們這邊配置活動項目，客端啟動遊戲就會出現錯誤


                    //if(eachSetting.CustomizedSettingKey == MerchantCustomizedSettingKeyEnum.GameSetting_EnableIDGPromotion_Quest)



                    //舊版v1 應該用不到了
                    //舊版v1 應該用不到了
                    //舊版v1 應該用不到了
                    //舊版v1 應該用不到了
                    //舊版v1 應該用不到了
                    //舊版v1 應該用不到了







                    //var referText = ((MerchantCustomizedSettingKeyEnum)eachSetting.CustomizedSettingKey).GetAttributeFromEnum<ReferenceText>()?.Value;

                    //if (string.IsNullOrWhiteSpace(referText))
                    //    continue;

                    //var parseServiceType = ExtensionOfEnum.GetEnumByTextValue(referText, AGGPromotionSettingServiceTypeEnum.None);

                    //if (parseServiceType == AGGPromotionSettingServiceTypeEnum.None)
                    //    continue;

                    //bool isDisable = eachSetting.SettingValue == "0";

                    ////2025/01/07 因為AGG又亂改規格 改成寫死
                    //List<string> targetSettingIDs = new List<string>
                    //{ 
                    //    ""
                    //};


                    //if(isDisable)
                    //{
                    //    var settingResult = aggService.GetAGGPromotionEventSettings(merchantID, parseServiceType);

                    //    //settingIDs 確實可能多組 (e.g. 每日任務)
                    //    targetSettingIDs = new List<string>();

                    //    if (settingResult == null || settingResult.Settings.IsEmptyOrNull())
                    //    {
                    //        targetSettingIDs.Add("");
                    //    }
                    //    else
                    //    {
                    //        targetSettingIDs = settingResult.Settings.Select(x => x.SettingId).ToList();
                    //    }
                    //}

                    //foreach (var settingID in targetSettingIDs)
                    //{
                    //    try
                    //    {
                    //        aggService.SetAGGPromotionSettingFlag(new AGGGameSettingFlagParameter
                    //        {
                    //            SettingID = settingID,
                    //            ServiceType = parseServiceType,
                    //            MerchantID = merchantID,
                    //            IsDisable = isDisable
                    //        });
                    //    }
                    //    catch
                    //    {

                    //    }
                    //}

                }
            }
        }


    }
}
