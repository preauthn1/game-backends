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
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace target.BL.Services
{
    public class IMChannelService : BaseAdminSysService, IIMChannelService
    {

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public IMChannelCustomizedSettingRepository IMChannelCustomizedSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }


        public PagedList<IMChannelViewModel> GetByPagingFilter(int userID, IMChannelRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter = filter ?? new IMChannelRequestParameterModel();
            filter.PageIndex = 1;
            filter.PageSize = filter.PageSize; //ps. 此功能不是走標準分頁 而是全撈出來

            if (filter.AssignMerchantID.HasValue)
            {
                var queryExist = IMChannelRepository.GetMerchantIMChannel(filter.AssignMerchantID.Value.ToListCollection());
                filter.IMChannelIDs = queryExist.FirstOrDefault()?.IMChannelID_Integer.ToListCollection();
            }

            if (filter.SpecialFilterType.HasValue)
            {
                if (filter.SpecialFilterType == (int)SpecialFilterTypeOfIMChannels.AssignProductionMerchantOnly)
                {
                    filter.IsAssignProductionMerchantOnly = true;
                }
            }

            var query = IMChannelRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<IMChannelEntity, IMChannelViewModel>(query);

            viewModels.Items = query.Items
                .Select(x =>
                {
                    var convert = x.ReturnInjecteObject<IMChannelViewModel>();
                    convert.IMChannelID = x.IMChannelID;
                    convert.ChannelName = x.DisplayName;
                    convert.InternalMemo = x.Memo;
                    convert.Memo2 = x.ThirdPartyChatID;
                    convert.Type = MultiLanguageTextHelper.GetEnumsCultureText((IMChannelTypeEnum)x.Type);

                    return convert;
                })
                .ToList();


            if (filter.PageSize >= 500)
            {
                viewModels.Items = viewModels.Items
                    .OrderBy(x =>
                    {
                        if (x.IMChannelID <= 19999)
                            return 1;

                        return 2;
                    })
                    .ThenByDescending(x => x.IMChannelID)
                    .ToList();
            }

            SetExtraProperty(viewModels.Items, filter);
            return viewModels;
        }

        public CRUDResultModel Create(int userID, IMChannelCreateRequestModel model)
        {
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ChannelName).Rule_NotEmptyText();

            bool isCreateSkypeNewChannel = model.Type == IMChannelTypeEnum.Skype.ToInteger();

            var queryExistItems = IMChannelRepository.GetByFilter(new QueryIMChannelRequestViewModel
            {
                IMChannelTypes = model.Type.ToListCollection()
            });

            string imChannelName = model.ChannelName;

            string imChannelNameTrim = imChannelName.TrimAndBlank();

            string memo = isCreateSkypeNewChannel ? "Skype" : "TG";

            var matchFirstTemplate = queryExistItems.FirstOrDefault();

            //查詢已存在channcel 需where過濾之前執行
            var existValue = queryExistItems
                .Where(x => x.DisplayName == imChannelName || x.DisplayName.TrimAndBlank() == imChannelNameTrim)
                .OrderBy(myValue =>
                {
                    if (isCreateSkypeNewChannel)
                    {
                        return myValue.IMChannelID >= 20000 ? 0 : 999;
                    }
                    else
                    {
                        return myValue.IMChannelID < 20000 ? 0 : 999;
                    }
                })
                .FirstOrDefault();


            if (existValue != null)
            {
                if (!string.IsNullOrWhiteSpace(model.TG_Chat_ID))
                {
                    existValue.ThirdPartyChatID = model.TG_Chat_ID;

                    IMChannelRepository.Update(existValue);
                }

                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
            }

            int maxValue = queryExistItems.Select(x => x.IMChannelID).Max();

            int newIMChannelID = (maxValue + 1);

            var now = DateTimeHelper.GetCurrentTime();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            IMChannelRepository.Create(new IMChannelEntity
            {
                IMChannelID = newIMChannelID,
                DisplayName = imChannelName,
                ThirdPartyChatID = model.TG_Chat_ID,
                Type = model.Type,
                Status = NormalActiveStatus.Enable.ToInteger(),
                CreateTime = now,
                CreateBy = currentUser?.UserName,
                UpdateTime = now,
            });

            //若符合條件 標記標籤 (ION頻道避免被顯示給richmond)
            if (isCreateSkypeNewChannel && CommonLogicHelper.HasIONSlotKeyword(imChannelName))
            {
                GeneralTagMappingRepository.Create(new GeneralTagMappingEntity
                {
                    TagID = GeneralTagEnum.NoneRichmondChannels.ToInteger(),
                    TargetID = newIMChannelID,
                    CreateTime = now,
                });
            }

            //自動創建一個IP防火牆配置
            int ipTemplateID = IPAddressListTemplateRepository.Create(new IPAddressListTemplateEntity
            {
                DisplayName = imChannelName,
                IPAddressList = "*",
                ReferIMChannelID = newIMChannelID,
                CreateTime = now,
                UpdateTime = now,
            });

            //默認防火牆配置
            IMChannelCustomizedSettingRepository.CreateOrUpdate(newIMChannelID, IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID, ipTemplateID.ToString());


            return new CRUDResultModel
            {
                ResultId = newIMChannelID
            };
        }


        public IMChannelEntity GetByChatID(string chatID, IMChannelTypeEnum type)
        {
            var query = IMChannelRepository.GetByChatID(chatID, type);
            return query;
        }


        public CRUDResultModel Update(int userID, IMChannelUpdateRequestModel model)
        {
            model.RuleFor(x => x.IMChannelID).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.ChannelName).Rule_NotEmptyText();

            var queryExist = IMChannelRepository.GetByIMChannelID(model.IMChannelID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExist.DisplayName = model.ChannelName;
            queryExist.Memo = model.Memo;

            string extraUpdateInfo = "";

            if (HasTechPermission_AllowToUseDevelopeFeature(userID))
            {
                if (!string.IsNullOrWhiteSpace(model.TargetChatID))
                {
                    if (model.TargetChatID != queryExist.ThirdPartyChatID)
                    {
                        extraUpdateInfo = $"{queryExist.ThirdPartyChatID} > {model.TargetChatID}";
                    }

                    queryExist.ThirdPartyChatID = model.TargetChatID;
                }
            }

            IMChannelRepository.Update(queryExist);

            string referIPTemplate = model.ReferIPAddressTemplateID.HasValue ? model.ReferIPAddressTemplateID.Value.ToString() : "";
            IMChannelCustomizedSettingRepository.CreateOrUpdate(model.IMChannelID, IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID, referIPTemplate);


            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateIMChannelBasic, model.IMChannelID);
            log.DebugInfo = extraUpdateInfo;

            bool isNotify = string.IsNullOrWhiteSpace(extraUpdateInfo) ? false : true;
            WriteOperateLog(log, isNotify);

            return new CRUDResultModel(true);
        }


        public CommonCustomizedSettingBatchEditModel GetIMChannelCutomziedSettings(int userID, int imChannelID)
        {
            var query = IMChannelRepository.GetById(imChannelID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var keys = ExtensionOfEnum.GetEnumAllValue<IMChannelCustomizedSettingKeyEnum>()
                .ToList();

            var resultModel = new CommonCustomizedSettingBatchEditModel();
            resultModel.OwnerID = query.IMChannelID;
            resultModel.OwnerName = query.DisplayName;
            resultModel.Items = new List<CustomizedSettingItemEditModel>();

            var settings = IMChannelCustomizedSettingRepository.GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
            {
                IMChannelIDs = query.IMChannelID.ToListCollection()
            });

            var ipTemplateDropdowns = IPAddressListTemplateRepository.GetByFilter(new QueryIPAddressListTemplateRequestViewModel
            {

            }, " Id , DisplayName ")
                .Select(x => new DropdownValueViewModel(x.DisplayName, x.Id.ToString()))
                .ToList();

            foreach (var key in keys)
            {
                int settingKey = key.ToInteger();
                var matchValue = settings.FirstOrDefault(x => x.SettingKey == settingKey);

                var myModel = new CustomizedSettingItemEditModel
                {
                    OnwerID = query.IMChannelID,
                    SettingKey = settingKey,
                    SettingKeyText = MultiLanguageTextHelper.GetEnumsCultureText(key),
                };

                SystemConfigSettingParseModel parseModel = null;

                if (matchValue != null)
                {
                    parseModel = new SystemConfigSettingParseModel(settingKey, matchValue.SettingValue);
                }
                else
                {
                    parseModel = new SystemConfigSettingParseModel(settingKey, key.GetAttributeFromEnum<SettingDefaultValue>()?.Value);
                }

                myModel.Value = parseModel.SettingValue;

                if (key == IMChannelCustomizedSettingKeyEnum.PreferLanguages)
                {
                    myModel.MultipleValues = parseModel.SettingValueOfListString;
                    myModel.DataDropdowns = new List<DropdownValueViewModel>
                    {
                        new DropdownValueViewModel("中文" , "zh-cn"),
                        new DropdownValueViewModel("英文(English)" , "en-us"),
                    };
                }
                else if (key == IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID)
                {
                    myModel.DataDropdowns = ipTemplateDropdowns;
                }
                resultModel.Items.Add(myModel);
            }

            return resultModel;
        }



        public void CreateOrUpdateSetting(int userID, CustomizedSettingItemEditModel model)
        {
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            if (model.SettingKey == (int)IMChannelCustomizedSettingKeyEnum.PreferLanguages)
            {
                model.RuleFor(x => x.MultipleValues).Rule_ModelNotNull();
                model.Value = string.Join(",", model.MultipleValues);
            }

            IMChannelCustomizedSettingRepository.CreateOrUpdate(model.OnwerID, (IMChannelCustomizedSettingKeyEnum)model.SettingKey, model.Value);

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateIMChannelSettings, model.OnwerID);
            log.Memo = $"{model.SettingKey}";

            WriteOperateLog(log);
        }


        private void SetExtraProperty(List<IMChannelViewModel> items, IMChannelRequestParameterModel filter)
        {
            var querySettingsDic = IMChannelCustomizedSettingRepository.GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
            {
                SettingKeys = new List<int>
                    {
                        IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID.ToInteger(),
                        IMChannelCustomizedSettingKeyEnum.PreferLanguages.ToInteger(),
                    }
            })
            .GroupBy(x => x.IMChannelID)
            .ToDictionary(x => x.Key, x => x);

            var allMerchantChannelsDic = IMChannelRepository.GetMerchantIMChannel(null, isReturnEmptyWhenPassEmptyIDs: false)
                .GroupBy(x => x.IMChannelID_Integer)
                .ToDictionary(x => x.Key, x => x.ToList());

            var queryTags = GeneralTagMappingRepository.GetEachMappingAndTagInfoByType(TagTypeEnum.IMChannel);

            int productionType = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger();

            var queryIPTemplate = IPAddressListTemplateRepository.GetByFilter(new QueryIPAddressListTemplateRequestViewModel(), " Id , DisplayName ");

            int typeOfTF = (int)MerchantWalletModeEnum.TransferMode;
            int typeOfSW = (int)MerchantWalletModeEnum.SingleWalletMode;

            foreach (var item in items)
            {
                item.Tags = new List<string>();
                item.PreferLangText = "中英";

                var mySettings = querySettingsDic.ContainsKey(item.IMChannelID);

                if (querySettingsDic.ContainsKey(item.IMChannelID))
                {
                    var matchIPTemplateSetting = querySettingsDic[item.IMChannelID].FirstOrDefault(x => x.SettingKey == (int)IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID);

                    if (matchIPTemplateSetting != null)
                    {
                        var parse = new SystemConfigSettingParseModel(0, matchIPTemplateSetting.SettingValue);
                        item.ReferIPTemplateID = parse.SettingValueOfInt;
                        item.ReferIPTemplateName = queryIPTemplate.FirstOrDefault(x => x.Id == item.ReferIPTemplateID)?.DisplayName;
                    }

                    var matchLangSetting = querySettingsDic[item.IMChannelID].FirstOrDefault(x => x.SettingKey == (int)IMChannelCustomizedSettingKeyEnum.PreferLanguages);

                    if (matchLangSetting != null)
                    {
                        var parse = new SystemConfigSettingParseModel(0, matchLangSetting.SettingValue);

                        bool isEN = parse.SettingValueOfListString.Any(x => x == "en-us");
                        bool isCN = parse.SettingValueOfListString.Any(x => x == "zh-cn");

                        if (isCN && isEN)
                        {

                        }
                        else if (isCN)
                        {
                            item.PreferLangText = "中";
                        }
                        else if (isEN)
                        {
                            item.PreferLangText = "英";
                        }
                    }
                }

                if (allMerchantChannelsDic.ContainsKey(item.IMChannelID))
                {
                    var merchants = allMerchantChannelsDic[item.IMChannelID];
                    item.IsMappingProductionMerchant = merchants.Any(z => z.MerchantType == productionType);
                    item.MerchantCount = merchants.Count();

                    if (item.MerchantCount > 0)
                    {
                        int numberOfTrasnfer = merchants.Where(x => x.MerchantWalletMode == typeOfTF).Count();
                        int numberOfSW = merchants.Where(x => x.MerchantWalletMode == typeOfSW).Count();

                        decimal rateTF = AppHelper.CalculateRate(numberOfTrasnfer, item.MerchantCount) * 100;
                        decimal rateSW = AppHelper.CalculateRate(numberOfSW, item.MerchantCount) * 100;

                        string myText = "";

                        if (rateTF == 100)
                        {
                            myText = "轉帳模式";
                        }
                        else if (rateSW == 100)
                        {
                            myText = "無縫模式";
                        }
                        else
                        {
                            myText = $"轉帳:{Math.Round(rateTF, 0)}% 無縫:{Math.Round(rateSW, 0)}%";
                        }

                        item.Tags.Add(myText);
                    }
                }

                var matchTags = queryTags.Where(x => x.TargetID == item.IMChannelID).ToList();

                if (matchTags.IsNotEmptyNull())
                {
                    item.Tags.AddRange(matchTags.Select(x => x.Name).Distinct().ToList());
                }
            }
        }


    }

}
