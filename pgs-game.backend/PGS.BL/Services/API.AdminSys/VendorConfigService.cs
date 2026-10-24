using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
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
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public class VendorConfigService : BaseAdminSysService, IVendorConfigService
    {

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public OneToManyCustomizedSettingRepository OneToManyCustomizedSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public CurrencyRepository CurrencyRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        public PagedList<VendorConfigViewModel> GetByPagingFilter(int userID, QueryVendorConfigApiRequestParameterModel filter)
        {
            filter.VendorIDs = filter.VendorID.ToIntegerListCollection();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var query = VendorConfigRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<VendorConfigEntity, VendorConfigViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public VendorConfigViewModel GetSingleVendorConfigDetail(int userID, int vendorConfigID)
        {           
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var query = VendorConfigRepository.GetPagingByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            var viewModels = ConvertToNewTypePagingObject<VendorConfigEntity, VendorConfigViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items , isIgnoreQueryListInfo: true);

            var merchantMappings = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection(),
                IsIncludeDeleteMerchant = false
            });

            List<int> merchantIDs = merchantMappings.Select(x => x.MerchantID).ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs, " MerchantID , Name , Currency ");

            foreach(var item in viewModels.Items)
            {
                item.MappingMerchantIDs = merchantIDs;
                item.MappingMerchants = queryMerchants.Select(x => new VendorConfigMappingMerchantViewModel
                    {
                        MerchantName = x.Name,
                        Currency = x.Currency,
                        MerchantID = x.MerchantID,
                    })
                    .ToList();
            }

            return viewModels.Items.FirstOrDefault();
        }



        public SystemConfigSettingPageViewModel GetVendorConfigSettins(int vendorConfigID)
        {
            var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            var queryAllSettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            var allEnums = ExtensionOfEnum.GetEnumAllValue<VendorConfigSettingEnum>().Select(x =>
                new SettingEnumStandardFormatConvertModel
                {
                    Group = x.GetAttributeFromEnum<DataCategoryAttribute>()?.Value ?? DataCategoryEnum.Common,
                    SettingKey = x.ToInteger(),
                    SettingKeyEnum = x
                })
                .ToList();

            //排除掉只有特定遊戲商才有的配置
            if (queryVendorConfig.VendorID != GameVendorTypeEnum.AGG.ToInteger())
            {
                allEnums = allEnums.Where(z => z.Group != DataCategoryEnum.GameVendor_AGGOnly).ToList();
            }

            var convertResult = SettingKeyEnumLogicHelper.ConvertToEditPageViewModel(allEnums);
            convertResult.OwnerID = vendorConfigID;

            //是否有打開編輯模式
            bool isEnableEdit = SettingKeyEnumLogicHelper.IsSuperAdminEnableEditForFiveMinutes();

            foreach (var group in convertResult.Groups)
            {
                foreach (var item in group.Items)
                {
                    var settingEnum = (VendorConfigSettingEnum)item.SettingKey;

                    var dbSetting = queryAllSettings.FirstOrDefault(x => x.SettingKey == item.SettingKey);

                    item.IsDisableEdit = !isEnableEdit;

                    var dataType = settingEnum.GetAttributeFromEnum<ReferType>();

                    if (SettingKeyEnumLogicHelper.IsDataTypeYesOrNoEnum(settingEnum))
                    {
                        item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<NormalYesOrNoEnum>();
                    }
                    else if (SettingKeyEnumLogicHelper.IsDataTypeNormalActiveStatus(settingEnum))
                    {
                        item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType<NormalActiveStatus>();
                    }
                    else if (dataType != null && dataType.Type.IsEnum)
                    {
                        item.DataDropdowns = CommonLogicHelper.GetDropdownValuesByEnumType(dataType.Type);
                    }

                    if (dbSetting != null)
                    {
                        if (string.IsNullOrWhiteSpace(dbSetting.Value) && item.DataDropdowns.IsNotEmptyNull())
                        {
                            //dropdown型的配置。如果db填空值、則以attribute的預設值為主
                        }
                        else
                        {
                            item.Value = dbSetting.Value;
                        }

                        if (!string.IsNullOrWhiteSpace(dbSetting.Memo))
                        {
                            item.Memo = $" ({dbSetting.Memo})";
                        }
                    }
                }

                group.IsAllDisableEdit = group.Items.All(z => z.IsDisableEdit);
            }


            return convertResult;
        }


        public void UpdateVendorConfigBasicInfo(int userID, VendorConfigBasicInfoUpdateModel model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Name).Rule_NotEmptyText();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var queryExist = VendorConfigRepository.GetById(model.VendorConfigID);
            queryExist.Name = model.Name;
            queryExist.SupportCurrency = model.SupportCurrency;
            queryExist.IsTestEnvironment = model.IsTestEnvironment;
            queryExist.IsSkipFetchSummary = model.IsSkipFetchSummary;

            VendorConfigRepository.Update(queryExist);

            CacheRemoveHelper.RemoveCache_WhenVendorConfigUpdate(model.VendorConfigID);            
        }


        public void UpdateVendorConfigSettins(int userID, SystemConfigSettingUpdateRequestModel model)
        {
            model.RuleFor(x => x.OwnerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Items).Rule_NotEmptyNull();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //檢查格式合法
            SettingKeyEnumLogicHelper.ValidateInputSettingValue<VendorConfigSettingEnum>(model.Items);

            foreach (var item in model.Items)
            {
                //迴圈編輯 (這功能不常使用 應該還好)
                var queryExist = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    VendorConfigIDs = model.OwnerID.ToListCollection(),
                    SettingKeys = item.SettingKey.ToListCollection()
                })
                    .FirstOrDefault();

                if (queryExist == null)
                {
                    VendorConfigSettingRepository.Create(new VendorConfigSettingEntity
                    {
                        SettingKey = item.SettingKey,
                        VendorConfigID = model.OwnerID,
                        Value = item.Value ?? "",
                        UpdateBy = currentUser?.UserName,
                        UpdateTime = DateTimeHelper.GetCurrentTime(),
                    });
                }
                else
                {
                    queryExist.Value = item.Value ?? "";
                    queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();

                    VendorConfigSettingRepository.Update(queryExist);
                }

                CacheRemoveHelper.RemoveCache_WhenVendorConfigUpdate(model.OwnerID);
            }
        }



        public CRUDResultModel CreateEmptyVendorConfig(int userID, VendorConfigCreateRequestModel model)
        {
            model.RuleFor(z => z.Name).Rule_NotEmptyText();
            model.RuleFor(z => z.GameVendorID).Rule_NotZeroNotMinus();

            var queryUser = GetCurrentOwnPermissionByCache(userID);

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                VendorIDs = model.GameVendorID.ToListCollection()
            });

            //是否為 這個廠商的第一筆配置
            bool isFirstVendorConfig = queryVendorConfig.IsEmptyOrNull();

            DateTime now = DateTimeHelper.GetCurrentTime();

            var insertEntity = new VendorConfigEntity
            {
                CreateBy = queryUser.UserName,
                UpdateBy = queryUser.UserName,
                CreateTime = now,
                UpdateTime = now,
                VendorID = model.GameVendorID,
                IsDefault = isFirstVendorConfig ? true : false,
                IsTestEnvironment = model.IsTestEnvironment,
                Name = model.Name,
            };

            int newConfigID = VendorConfigRepository.Create(insertEntity);

            var allEnums = ExtensionOfEnum.GetEnumAllValue<VendorConfigSettingEnum>();

            foreach (VendorConfigSettingEnum key in allEnums)
            {
                //先建好各種配置 塞空值
                VendorConfigSettingRepository.Create(new VendorConfigSettingEntity
                {
                    SettingKey = key.ToInteger(),
                    UpdateBy = queryUser.UserName,
                    UpdateTime = now,
                    VendorConfigID = newConfigID,
                    Value = "",
                });
            }

            //若是live遊戲商。會額外產生其他資料
            CreateCasinoVipMappingForLiveGameVendor(insertEntity, newConfigID);

            return new CRUDResultModel
            {
                ResultId = newConfigID
            };
        }

        public void DeleteVendorConfig(int userID, int vendorConfigID)
        {
            var queryUser = GetCurrentOwnPermissionByCache(userID);

            var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryVendorConfig.Status = NormalActiveStatus.PermanentDelete.ToInteger();

            VendorConfigRepository.Update(queryVendorConfig);
        }


        public void CopyAndCreateNewVendorConfig(int userID, VendorConfigCopyRequestModel model)
        {
            model.RuleFor(z => z.Name).Rule_NotEmptyText();
            model.RuleFor(z => z.CopyFromVendorConfigID).Rule_NotZeroNotMinus();

            var queryUser = GetCurrentOwnPermissionByCache(userID);

            var queryVendorConfig = VendorConfigRepository.GetById(model.CopyFromVendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            DateTime now = DateTimeHelper.GetCurrentTime();
            var newEntity = queryVendorConfig.ReturnInjecteObject<VendorConfigEntity>();
            newEntity.Name = model.Name;
            newEntity.CreateTime = now;
            newEntity.CreateBy = queryUser.UserName;
            newEntity.UpdateTime = now;
            newEntity.UpdateBy = queryUser.UserName;
            newEntity.IsDefault = false;
            newEntity.IsTestEnvironment = false;

            if (!string.IsNullOrWhiteSpace(model.SupportCurrency))
            {
                newEntity.SupportCurrency = model.SupportCurrency.Trim();
            }

            int newID = VendorConfigRepository.Create(newEntity);
            newEntity.VendorConfigID = newID;

            var querySettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = queryVendorConfig.VendorConfigID.ToListCollection()
            });

            if (querySettings.IsEmptyOrNull())
                return;

            foreach (var item in querySettings)
            {
                item.Id = 0;
                item.VendorConfigID = newID;
                item.UpdateTime = now;
                item.UpdateBy = queryUser.UserName;
            }

            //如果有填值
            if(!string.IsNullOrWhiteSpace(model.AssignVendorID))
            {
                var matchAssignID = querySettings.FirstOrDefault(x => x.SettingKey == (int)VendorConfigSettingEnum.MainAssignVendorID);
                matchAssignID.Value = model.AssignVendorID;
            }

            VendorConfigSettingRepository.CreateMany(querySettings);

            //如果有填值 (secretKey)
            if (!string.IsNullOrWhiteSpace(model.AssignSecretKey))
            {
                VendorConfigSettingRepository.CreateOrUpdate(new VendorConfigSettingEntity
                {
                    VendorConfigID = newEntity.VendorConfigID,
                    SettingKey = VendorConfigSettingEnum.MainSecretKeyOrApiToken.ToInteger(),
                    UpdateTime = DateTimeHelper.GetCurrentTime(),
                    UpdateBy = queryUser.UserName,
                    Value = model.AssignSecretKey,
                });
            }

            //若是live遊戲商。會額外產生其他資料
            CreateCasinoVipMappingForLiveGameVendor(newEntity, newID);
        }


        private void CreateCasinoVipMappingForLiveGameVendor(VendorConfigEntity newEntity, int newVendorConfigID)
        {
            if (newEntity.VendorID == (int)GameVendorTypeEnum.AGGLive)
            { 
                //v2版本的儲存方法
                var queryParentType = OneToManyCustomizedSettingRepository.GetUniqueByType(OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_AGGLiveV2);

                OneToManyCustomizedSettingItemRepository.Create(new OneToManyCustomizedSettingItemEntity
                {
                    SettingKey = 0,
                    SubCategory = newVendorConfigID.ToString(),
                    SettingValue = "0,1,2,3,4,5,6,7,8,9",
                    OneToManyCustomizedSettingID = queryParentType.OneToManyCustomizedSettingID,
                });

                return;
            }
            else if(newEntity.VendorID == (int)GameVendorTypeEnum.Galaxy)
            {
                var vipLevelSetting = OneToManyCustomizedSettingRepository.GetUniqueByType(OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_Galaxy);

                List<string> vipLevels = new List<string>
                {
                    "VIP000",
                    "VIP001",
                    "VIP002",
                    "VIP003",
                    "VIP004",
                };

                var insertItems = new List<OneToManyCustomizedSettingItemEntity>();

                foreach(var levelID in vipLevels)
                {
                    insertItems.Add(new OneToManyCustomizedSettingItemEntity
                    {
                        SettingKey = newVendorConfigID,
                        SettingValue = "10,5000",
                        SubCategory = levelID,
                        OneToManyCustomizedSettingID = vipLevelSetting.OneToManyCustomizedSettingID,
                        Memo = ""
                    });
                }

                OneToManyCustomizedSettingItemRepository.CreateMany(insertItems);
            }
        }


        public void SyncDefaultVendorConfigMapping(int vendorConfigID)
        {
            var queryVendorConfig = VendorConfigRepository.GetById(vendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            //找出針對這個遊戲商 還沒配置的商戶
            List<int> queryMerchantIDs = VendorConfigMerchantMappingRepository.GetNotSettingMerchants(queryVendorConfig.VendorID);

            if (queryMerchantIDs.IsEmptyOrNull())
                return;

            var queryMerchants = MerchantRepository.GetByIds(queryMerchantIDs);

            var restrictCurrency = GetRestrictCurrency(queryVendorConfig.SupportCurrency);

            List<VendorConfigMerchantMappingEntity> isnertItems = new List<VendorConfigMerchantMappingEntity>();
            foreach (var merchant in queryMerchants)
            {
                if (restrictCurrency.IsRestrictCurrency)
                {
                    if (!restrictCurrency.SupportCurrency.Contains(merchant.Currency))
                    {
                        continue;
                    }
                }

                isnertItems.Add(new VendorConfigMerchantMappingEntity
                {
                    MerchantID = merchant.MerchantID,
                    VendorConfigID = queryVendorConfig.VendorConfigID,
                    VendorID = queryVendorConfig.VendorID
                });
            }

            //把這些商戶塞預設值
            VendorConfigMerchantMappingRepository.CreateMany(isnertItems);

            CacheRemoveHelper.RemoveCache_WhenVendorConfigUpdate(vendorConfigID);
        }

        public void AddVendorConfigMerchantMapping(VendorConfigMappingAddRequestModel model)
        {
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryMerchant = MerchantRepository.GetById(model.MerchantID);

            var restrictCurrency = GetRestrictCurrency(queryVendorConfig.SupportCurrency);

            if (restrictCurrency.IsRestrictCurrency)
            {
                if (!restrictCurrency.SupportCurrency.Contains(queryMerchant.Currency))
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_CurrencyNotMatch);
                }
            }

            var queryExistVendorMapping = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                MerchantIDs = model.MerchantID.ToListCollection(),
                VendorIDs = queryVendorConfig.VendorID.ToListCollection()
            });

            if (queryExistVendorMapping.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            VendorConfigMerchantMappingRepository.Create(new VendorConfigMerchantMappingEntity
            {
                MerchantID = model.MerchantID,
                VendorConfigID = model.VendorConfigID,
                VendorID = model.VendorConfigID
            });
        }

        public MerchantVendorConfigSettingsModel GetMerchantVendorConfigMappings(int userID, int merchantID)
        {
            var queryMerchant = MerchantRepository.GetById(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            string myCurrency = queryMerchant.Currency;

            bool isBetaCurrency = myCurrency == CurrencyEnum.BETA.ToString();

            var enableGameIDs = MerchantGameSettingRepository.GetMerchantAvailableGameIDs(merchantID);

            var queryMappings = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                MerchantIDs = merchantID.ToListCollection()
            });

            var queryAllVendorConfigs = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
            {
                IsIncludeDeleteData = false
            });

            var availableVendors = AppHelper.GetStillAvailableGameVendors().Select(z => z.ToInteger()).ToList();

            var mappingItems = queryMappings
                .Where(x => availableVendors.Contains(x.VendorID))
                .Select(z =>
            {
                int vendorID = z.VendorID;
                var convert = new MerchantVendorConfigSettingItemViewModel
                {
                    EnableGames = 0,
                    GameVendor = vendorID,
                    GameVendorName = MultiLanguageTextHelper.GetEnumsCultureText((GameVendorTypeEnum)vendorID),
                    VendorConfigID = z.VendorConfigID,
                    VendorConfigName = queryAllVendorConfigs.FirstOrDefault(z => z.VendorConfigID == z.VendorConfigID)?.Name,
                    IsAssignAnyGames = enableGameIDs.Any(gameID => AppHelper.GetGameVendorByGameID(gameID) == vendorID),
                    DropdownItems = queryAllVendorConfigs
                        .Where(x => isBetaCurrency || string.IsNullOrWhiteSpace(x.SupportCurrency) || x.SupportCurrency.Contains(myCurrency))
                        .Where(z => z.VendorID == vendorID)
                        .Select(z => new DropdownValueViewModel
                        {
                            Text = z.Name,
                            Value = z.VendorConfigID.ToString(),
                            ValueInt = z.VendorConfigID
                        })
                        .ToList()
                };

                return convert;
            })
            .OrderBy(z => z.GameVendor)
            .ToList();

            return new MerchantVendorConfigSettingsModel
            {
                MerchantID = merchantID,
                MerchantName = queryMerchant.Name,
                MerchantType = queryMerchant.Type,
                MerchantTypeText = MultiLanguageTextHelper.GetEnumsCultureText((MerchantTypeEnum)queryMerchant.Type),
                Mappings = mappingItems
            };
        }


        public void UpdateMerchantVendorConfigMapping(int userID, MerchantVendorConfigSettingUpdateViewModel model)
        {
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameVendor).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var queryMerchant = MerchantRepository.GetById(model.MerchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var beforeValue = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                VendorIDs = model.GameVendor.ToListCollection(),
                MerchantIDs = model.MerchantID.ToListCollection()
            });

            var afterVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            if (beforeValue.IsNotEmptyNull() && SettingHelper.IsProduction)
            {
                var beforeVendorConfig = VendorConfigRepository.GetById(beforeValue.FirstOrDefault().VendorConfigID);

                if (beforeVendorConfig.VendorID != afterVendorConfig.VendorID)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("VendorID not match"));
                }

                string msg = $"user: {userID} 修改了商戶：{queryMerchant.MerchantID}【{queryMerchant.Name}】 VendorConfig對應。{Environment.NewLine}{beforeVendorConfig.VendorConfigID} `{beforeVendorConfig.Name}` => {afterVendorConfig.VendorConfigID} `{afterVendorConfig.Name}` ";
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_adminsys,
                    Message = msg
                });
            }

            VendorConfigMerchantMappingRepository.DeleteByModel(new
            {
                MerchantID = model.MerchantID,
                VendorID = model.GameVendor
            });

            VendorConfigMerchantMappingRepository.Create(new VendorConfigMerchantMappingEntity
            {
                MerchantID = model.MerchantID,
                VendorID = model.GameVendor,
                VendorConfigID = model.VendorConfigID
            });

            CacheRemoveHelper.RemoveCache_WhenVendorConfigUpdate(model.VendorConfigID);
            CacheRemoveHelper.RemoveCache_WhenVendorConfigMerchantMappingUpdate(model.MerchantID);
            return;
        }


        public void DeleteVendorConfigMerchantMapping(VendorConfigDeleteRequestModel model)
        {
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var query = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                MerchantIDs = model.MerchantID.ToListCollection(),
                VendorConfigIDs = model.VendorConfigID.ToListCollection()
            })
                .FirstOrDefault();

            if (query == null)
                return;

            var deleteKey = new
            {
                MerchantID = model.MerchantID,
                VendorConfigID = model.VendorConfigID
            };

            VendorConfigMerchantMappingRepository.DeleteByModel(deleteKey);
        }


        private VendorConfigViewModel ConvertToViewModel(VendorConfigEntity query)
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<VendorConfigViewModel>();
            convert.VendorName = MultiLanguageTextHelper.GetGameVendorDisplayName(query.VendorID);

            return convert;
        }



        private (bool IsRestrictCurrency, List<string> SupportCurrency) GetRestrictCurrency(string supportCurrency)
        {
            if (string.IsNullOrWhiteSpace(supportCurrency))
            {
                return (false, new List<string>());
            }

            List<string> availableCurrency = supportCurrency.Split(',')
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .ToList();

            return (availableCurrency.Count() > 0, availableCurrency);
        }



        private void SetExtraProperty(List<VendorConfigViewModel> models , bool isIgnoreQueryListInfo = false)
        {
            foreach(var item in models)
            {
                item.MappingMerchantIDs = new List<int>();
            }

            //有些資訊只會在列表頁查詢 (減少回應時間)
            if (!isIgnoreQueryListInfo)
            {
                var merchantMappingsDic = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
                    {
                        IsIncludeDeleteMerchant = false
                    })
                    .GroupBy(x => x.VendorConfigID)
                    .ToDictionary(x => x.Key, x => x.ToList());

                var queryVipLevelOfGalaxy = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_Galaxy.ToInteger()
                });

                var assignSettingDic = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    SettingKeys = VendorConfigSettingEnum.MainAssignVendorID.ToInteger().ToListCollection()
                })
                    .GroupBy(x => x.VendorConfigID)
                    .ToDictionary(x => x.Key, x => x.FirstOrDefault()?.Value);

                var queryCurrency = CurrencyRepository.GetAll();

                foreach (var item in models)
                {
                    if (item.VendorID == (int)GameVendorTypeEnum.Galaxy)
                    {
                        item.IsVipLevelSettingNotExist = queryVipLevelOfGalaxy.Where(x => x.SettingKey == item.VendorConfigID).Count() < 2;
                        string qqqq = "";
                    }
                    else if (item.VendorID == (int)GameVendorTypeEnum.AGGLive)
                    {
                        //item.IsVipLevelSettingNotExist = !queryVipLevelAGGLive.Any(x => x.SubCategory == item.VendorConfigID.ToString());
                    }

                    item.AssignVendorID = assignSettingDic.GetDictionaryOrDefaultValue(item.VendorConfigID, "");

                    if (item.VendorID == (int)GameVendorTypeEnum.SlotMania)
                    {
                        item.AssignGameVendorDefineName = "appID";
                    }

                    if (merchantMappingsDic.ContainsKey(item.VendorConfigID))
                    {
                        item.NumberOfMerchants = merchantMappingsDic[item.VendorConfigID].Count();
                    }

                    if (item.SupportCurrency.IsNotEmptyTextNotNull())
                    {
                        item.ExchangeRateToTWD = queryCurrency.FirstOrDefault(x => x.Currency == item.SupportCurrency)?.ExchangeRateToTWD ?? 0;
                    }
                }
            }

            return;
        }


    }

}
