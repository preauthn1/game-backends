using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.Core.Helper;
using target.DA.Repository;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public class OneToManyCustomizedSettingService : BaseAdminSysService, IOneToManyCustomizedSettingService
    {

        public OneToManyCustomizedSettingRepository OneToManyCustomizedSettingRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }


        public PagedList<OneToManyCustomizedSettingViewModel> GetByPagingFilter(int userID, QueryOneToManyCustomizedSettingApiRequestParameterModel filter)
        {
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            //OneToManyCustomizedSettingTypeEnum 特定type排序較前
            filter.OrderByColumn = @" (CASE Type 
                    WHEN 5333 THEN 999999 
                    WHEN 6555 THEN 900000 
                    WHEN 5777 THEN 710000 
                    WHEN 5779 THEN 710000 
                    WHEN 5780 THEN 700000 
                    WHEN 8327 THEN 10 
                    ELSE (10 + Type) END ) ";


            filter.IsSkipCheckOrderByInjection = true;

            var query = OneToManyCustomizedSettingRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<OneToManyCustomizedSettingEntity, OneToManyCustomizedSettingViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<OneToManyCustomizedSettingViewModel>();
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }

        public void Create(int userID, OneToManyCustomizedSettingCreateRequestModel model)
        {
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();

            var enumValue = (OneToManyCustomizedSettingTypeEnum)model.Type;

            if (!enumValue.IsDefinedValue())
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);

            bool isUnique = enumValue.IsAttributeExist<IsUnique>();

            if (isUnique)
            {
                var queryExist = OneToManyCustomizedSettingRepository.GetByFilter(new QueryOneToManyCustomizedSettingRequestViewModel
                {
                    Types = enumValue.ToInteger().ToListCollection()
                });

                if (queryExist.IsNotEmptyNull())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }
            }

            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            DateTime now = DateTimeHelper.GetCurrentTime();

            int newID = OneToManyCustomizedSettingRepository.Create(new OneToManyCustomizedSettingEntity
            {
                CreateTime = now,
                DisplayName = model.DisplayName,
                Type = model.Type,
                Memo = model.Memo,
                OwnerID = model.OwnerID,
                OwnerIDMemo = model.OwnerIDMemo,
            });

            List<OneToManyCustomizedSettingItemEntity> insertItems = new List<OneToManyCustomizedSettingItemEntity>();

            if (!string.IsNullOrWhiteSpace(model.DefaultSubCategory))
            {
                var split = model
                    .DefaultSubCategory.Split(',')
                    .Where(x => x.IsNotEmptyTextNotNull())
                    .ToList();

                foreach (var text in split)
                {
                    string defaultValue = "預先產生_" + text;

                    //為了讓操作更簡化的一些預設值
                    if (text.Contains("5"))
                    {
                        defaultValue = "5";
                    }
                    else if (text.Contains("4"))
                    {
                        defaultValue = "4";
                    }
                    else if (text.Contains("3"))
                    {
                        defaultValue = "3";
                    }
                    else if (text.Contains("2"))
                    {
                        defaultValue = "2";
                    }
                    else if (text.Contains("1"))
                    {
                        defaultValue = "1";
                    }
                    else if (text.Contains("0"))
                    {
                        defaultValue = "0";
                    }

                    insertItems.Add(new OneToManyCustomizedSettingItemEntity
                    {
                        SettingKey = OneToManyCustomizedSettingItemTypeEnum.None.ToInteger(),
                        SettingValue = defaultValue,
                        SubCategory = text.Trim(),
                        OneToManyCustomizedSettingID = newID
                    });
                }
            }

            OneToManyCustomizedSettingItemRepository.CreateMany(insertItems);
            return;
        }


        public SystemConfigSettingPageViewModel GetSettingItems(int userID, int oneToManyCustomizedSettingID)
        {
            var query = OneToManyCustomizedSettingRepository.GetById(oneToManyCustomizedSettingID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var querySettingItems = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingID = oneToManyCustomizedSettingID,
            });

            List<SystemConfigSettingGroupViewModel> items = querySettingItems
                .GroupBy(z => z.SettingKey)
                .Select(z =>
                {
                    var myModel = new SystemConfigSettingGroupViewModel
                    {
                        GroupID = z.Key,
                        GroupName = MultiLanguageTextHelper.GetEnumsCultureText((OneToManyCustomizedSettingItemTypeEnum)z.Key),
                        Items = z.OrderBy(x =>
                        {
                            if (x.SubCategory.IsNumeric())
                            {
                                return Int32.Parse(x.SubCategory);
                            }
                            else
                            {
                                return 0;
                            }
                        })
                        .ThenBy(x => x.SubCategory)
                        .Select(x => new SystemConfigSettingViewModel
                        {
                            SettingKey = z.Key,
                            SettingKeyText = x.SubCategory,
                            SettingKeyDisplayName = x.SubCategory,
                            Value = x.SettingValue,
                            Memo = x.Memo,
                            Debug1 = GenerateUpdateSQL(x),
                            DataTypeName = CommonLogicHelper.GetReferenceDataTypeFromEnum(new ReferType(typeof(string)))?.TypeName
                        })
                        .ToList()
                    };

                    //好爛的寫法。總之把這區間視為服務器代號
                    if(myModel.GroupID >= 50000 && myModel.GroupID <= 55000)
                    {
                        //迴圈查SQL 但是這功能不常用 就這樣吧
                        var queryVendorConfig = VendorConfigRepository.GetById(myModel.GroupID);

                        myModel.GroupName = $"限紅組別設置 ({myModel.GroupID}｜{queryVendorConfig.Name})";
                    }

                    return myModel;
                }).ToList();

            string sql1 = SQLHelper.GenerateInsertDataCommandByRows("[dbo].[OneToManyCustomizedSetting]", new List<OneToManyCustomizedSettingEntity> { query }) + Environment.NewLine + Environment.NewLine;
            string sql2 = SQLHelper.GenerateInsertDataCommandByRows("[dbo].[OneToManyCustomizedSettingItem]", querySettingItems) + Environment.NewLine;
            string selectNewID = "DECLARE @newID int" + Environment.NewLine + "SELECT @newID = CAST(SCOPE_IDENTITY() as bigint)" + Environment.NewLine + Environment.NewLine;

            string sql = sql1 + selectNewID + sql2.Replace($"'{query.OneToManyCustomizedSettingID}'", "@newID");
            string selectSQL = @$"
-- 基本查詢
SELECT * FROM OneToManyCustomizedSettingItem WHERE OneToManyCustomizedSettingID = {oneToManyCustomizedSettingID} " + Environment.NewLine;

            selectSQL += $@"
-- 複製某一個來源的設定 (下方範例為某個settingKey)
INSERT INTO OneToManyCustomizedSettingItem
SELECT {oneToManyCustomizedSettingID} , XXXXXXX , SubCategory , SettingValue , 0 , '' , '' , '' FROM OneToManyCustomizedSettingItem 
WHERE OneToManyCustomizedSettingID = {oneToManyCustomizedSettingID} AND  SettingKey = YYYYYYYY
{Environment.NewLine}";

            return new SystemConfigSettingPageViewModel
            {
                SqlInsertScript = sql,
                SqlQueryScript = selectSQL,
                OwnerID = oneToManyCustomizedSettingID,
                Groups = items
            };
        }


        private string GenerateUpdateSQL(OneToManyCustomizedSettingItemEntity item)
        {
            string newLine = Environment.NewLine;
            string sql = $@"UPDATE [dbo].[OneToManyCustomizedSettingItem] SET {newLine}" +
                              $"  SettingValue = '{item.SettingValue}', {newLine}"  +
                              $"  SubCategory = '{item.SubCategory}', {newLine}" +
                              $"  Memo = '{item.Memo}', {newLine}" +
                              $"  Memo2 = '{item.Memo2}' {newLine}" +
                           $"WHERE OneToManyCustomizedSettingID = {item.OneToManyCustomizedSettingID} AND SettingKey = {item.SettingKey} AND SubCategory = '{item.SubCategory}' " + Environment.NewLine;

            return sql;
        }


        public void UpdateSettingItems(int userID, SystemConfigSettingUpdateRequestModel model)
        {
            model.RuleFor(z => z.OwnerID).Rule_NotZeroNotMinus();

            // (不適用一對多的結構) 檢查格式合法
            //SettingKeyEnumLogicHelper.CheckIsValidSettingValueInput<SystemConfigSettingEnum>(model.Items);

            var queryParent = OneToManyCustomizedSettingRepository.GetById(model.OwnerID);

            if (SettingHelper.IsProduction)
            {
                CheckDuplicateValues(queryParent.Type, model);
            }

            //防呆 & 格式處理檢查
            foreach(var item in model.Items)
            {
                if(!string.IsNullOrWhiteSpace(item.Value))
                {
                    if (queryParent.Type == (int)OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder)
                    {
                        item.Value = item.Value.Replace(".", "");

                        var parse = new SystemConfigSettingParseModel(0, item.Value);

                        if(parse.SettingValueOfListInt.IsEmptyOrNull())
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError);
                        }

                        //排除重複的gameID
                        HashSet<int> listOfGameIDs = new HashSet<int>();

                        parse.SettingValueOfListInt.ForEach(x => listOfGameIDs.Add(x));

                        item.Value = string.Join(",", listOfGameIDs);
                    }
                }

            }

            foreach (var item in model.Items)
            {
                //迴圈編輯 (這功能不常使用 應該還好)
                var queryExist = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingID = model.OwnerID,
                    SettingKeys = item.SettingKey.ToListCollection(),
                    SubCategory = item.SettingKeyText

                }).FirstOrDefault();

                if (queryExist == null)
                {
                    OneToManyCustomizedSettingItemRepository.Create(new OneToManyCustomizedSettingItemEntity
                    {
                        SettingKey = item.SettingKey,
                        SubCategory = item.SettingKeyText,
                        OneToManyCustomizedSettingID = model.OwnerID,
                        SettingValue = item.Value
                    });
                }
                else
                {
                    queryExist.SettingValue = item.Value;
                    OneToManyCustomizedSettingItemRepository.Update(queryExist);
                }
            }
        }


        public void CheckDuplicateValues(int oneToManySettingType, SystemConfigSettingUpdateRequestModel model)
        {
            if (model.Items.IsEmptyOrNull())
                return;
            
            //*舊版寫法有部分移除了
        }


        public void UpdateReportDateMemoSettings(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.DateText).Rule_NotEmptyText();

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            var targetType = OneToManyCustomizedSettingTypeEnum.ThirdPartyAccounting_MemoInformation;

            var query = OneToManyCustomizedSettingRepository.GetByFilter(new QueryOneToManyCustomizedSettingRequestViewModel
            {
                Types = targetType.ToInteger().ToListCollection(),
                OwnerID = model.VendorConfigID
            })
                .FirstOrDefault();

            if (query == null)
            {
                //若沒資料自動建一筆
                OneToManyCustomizedSettingRepository.Create(new OneToManyCustomizedSettingEntity
                {
                    Type = targetType.ToInteger(),
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    DisplayName = "三方數據報表備註",
                    OwnerID = model.VendorConfigID,
                    OwnerIDMemo = queryVendorConfig.Name,
                    Memo = ""
                });

                query = OneToManyCustomizedSettingRepository.GetByFilter(new QueryOneToManyCustomizedSettingRequestViewModel
                {
                    Types = targetType.ToInteger().ToListCollection(),
                    OwnerID = model.VendorConfigID
                })
                .FirstOrDefault();
            }

            var querySettingItem = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingID = query.OneToManyCustomizedSettingID,
                SubCategory = model.DateText
            })
                .FirstOrDefault();

            model.Memo = model.Memo.Truncate(500);

            if (querySettingItem == null)
            {
                OneToManyCustomizedSettingItemRepository.Create(new OneToManyCustomizedSettingItemEntity
                {
                    SubCategory = model.DateText,
                    OneToManyCustomizedSettingID = query.OneToManyCustomizedSettingID,
                    SettingKey = 0,
                    SettingValue = model.Memo.Truncate(500),
                });
            }
            else
            {
                querySettingItem.SettingValue = model.Memo;
                OneToManyCustomizedSettingItemRepository.Update(querySettingItem);
            }
        }


        public void InsertNewEmptyItem(int userID, OneToManySettingEazyRequestModel model)
        {
            model.RuleFor(x => x.OneToManyCustomizedSettingID).Rule_NotZeroNotMinus();

            var queryExistItems = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingID = model.OneToManyCustomizedSettingID
            });

            List<int> numbers = queryExistItems
                .Select(x=> x.SubCategory.Replace("VIP" , ""))
                .Where(x => x.IsNumeric())
                .Select(x => Int32.Parse(x))
                .ToList();

            var maxSequence = 0;

            var insertNew = new OneToManyCustomizedSettingItemEntity
            {
                OneToManyCustomizedSettingID = model.OneToManyCustomizedSettingID,
                SettingKey = 0,
                SettingValue = model.SettingValue ?? "",
                SubCategory = (maxSequence + 1).ToString(),
                Memo = model.Memo,
                IsDefault = false
            };

            bool isVIPLevel = false;

            //已既有資料為基準值
            if(queryExistItems.IsNotEmptyNull())
            {
                var matchFirst = queryExistItems.FirstOrDefault();

                insertNew.SettingKey = matchFirst.SettingKey;

                if(string.IsNullOrWhiteSpace(model.Memo))
                {
                    insertNew.Memo = matchFirst.Memo;
                }

                isVIPLevel = (matchFirst.SubCategory ?? "").Contains("VIP");
            }

            if (numbers.IsNotEmptyNull())
            {
                maxSequence = numbers
                    .OrderByDescending(x => x)
                    .FirstOrDefault();
            }
            else
            {
                maxSequence = queryExistItems.Count() + 1;
            }

            if(isVIPLevel)
            {
                int nextValue = (maxSequence + 1);

                if(nextValue >= 10)
                {
                    insertNew.SubCategory = $"VIP0{nextValue}";
                }
                else
                {
                    insertNew.SubCategory = $"VIP00{nextValue}";
                }
            }
            else
            {
                insertNew.SubCategory = (maxSequence + 1).ToString();
            }

            if(!string.IsNullOrWhiteSpace(model.SubCategory))
            {
                insertNew.SubCategory = model.SubCategory;
            }

            OneToManyCustomizedSettingItemRepository.Create(insertNew);
        }


        private void SetExtraProperty(List<OneToManyCustomizedSettingViewModel> items)
        {
            foreach (var item in items)
            {
                item.TypeName = MultiLanguageTextHelper.GetEnumsCultureText((OneToManyCustomizedSettingTypeEnum)item.Type);
            }
        }

    }

}
