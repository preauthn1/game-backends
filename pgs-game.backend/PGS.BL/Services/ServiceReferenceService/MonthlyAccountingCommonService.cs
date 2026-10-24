using game.Entity.Game;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public class MonthlyAccountingCommonService : BaseAdminSysService, IMonthlyAccountingCommonService
    {

        public AccountingAdjustmentRuleRepository AccountingAdjustmentRuleRepository { get; set; }

        public AccountingAdjustmentRuleGroupAssignRepository AccountingAdjustmentRuleGroupAssignRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }



        public AccountingAdjustmentRuleViewModel GetAccountingAdjustmentRules(int userID)
        {
            var queryRules = AccountingAdjustmentRuleRepository.GetByFilter(new QueryAccountingAdjustmentRuleRequestViewModel
            {

            });

            queryRules = queryRules
                .Where(x => x.Type != -1)
                .ToList();

            var queryRuleAssignGroups = AccountingAdjustmentRuleGroupAssignRepository.GetAdjustmentRuleAndAssinGroups();

            List<AccountingAdjustmentRuleItemViewModel> items = new List<AccountingAdjustmentRuleItemViewModel>();

            var now = DateTimeHelper.GetCurrentTime();

            foreach (var eachEntity in queryRules)
            {
                var instance = eachEntity.ReturnInjecteObject<AccountingAdjustmentRuleItemViewModel>();
                instance.RuleID = eachEntity.Id;
                instance.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((AccountingAdjustRuleType)eachEntity.Type);
                instance.AdjustValuePercentage = eachEntity.AdjustValue * 100;
                instance.NumberOfMerchantID = instance.MerchantIDs.SeperateToListInt().Distinct().Count();

                if(instance.NumberOfMerchantID > 0 && instance.NumberOfMerchantID <= 5)
                {
                    //嘗試顯示更多商戶資訊
                    var merchantIDs = instance.MerchantIDs.SeperateToListInt().Distinct().ToList();
                    if (merchantIDs.IsNotEmptyNull())
                    {
                        var detailMerchants = MerchantRepository.GetByIds(merchantIDs);
                        instance.MerchantsDisplayInfo = string.Join("、", detailMerchants.Select(x => $"{x.MerchantID} ({x.Name})"));
                    }
                }

                if (eachEntity.BeginTime.HasValue)
                {
                    instance.BeginTimeTS = eachEntity.BeginTime.Value.ConvertToUnixTimestamp();
                    instance.BeginTimeTwText = eachEntity.BeginTime.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
                }

                if (eachEntity.EndTime.HasValue)
                {
                    //過期太久的才顯示expired
                    if (now.AddDays(-20) > eachEntity.EndTime)
                    {
                        instance.IsExpired = true;
                    }

                    instance.EndTimeTS = eachEntity.EndTime.Value.ConvertToUnixTimestamp();
                    instance.EndTimeTwText = eachEntity.EndTime.Value.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
                }

                instance.AssignGroups = queryRuleAssignGroups
                    .Where(z => z.AccountingAdjustmentRuleID == eachEntity.Id)
                    .Select(x => x.GroupName)
                    .Distinct()
                    .ToList();

                instance.IsHintEmptyAssign = instance.AssignGroups.IsEmptyOrNull();
                items.Add(instance);
            }

            items = items
                .OrderBy(x => x.IsExpired ? 999 : 0)
                .ThenBy(z => z.SortSequence ?? 9999)
                .ThenBy(x => x.Type)
                .ThenBy(x => x.NameCN)
                .ToList();

            var visibleBeginTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * 210);                

            //超過太久時間的就不顯示了
            items = items
               .Where(x => !x.IsExpired || x.BeginTime >= visibleBeginTime)           
               .ToList();

            var aggregativeGroupItems = new List<AccountingAdjustmentRuleAggregativeGroupMappingItems>();

            var enums = ExtensionOfEnum.GetEnumAllValue<AccountingAdjustmentRuleAggregativeGroupEnum>();

            foreach (var myEnum in enums)
            {
                if (myEnum.IsAttributeExist<Invisible>())
                    continue;

                int myGroupID = myEnum.ToInteger();
                var queryMyRules = queryRules.Where(x => x.AggregativeGroup == myGroupID).ToList();

                var myInstance = new AccountingAdjustmentRuleAggregativeGroupMappingItems
                {
                    AggregativeGroup = myGroupID,
                    Name = myEnum.GetDescription(),
                    DropdownItems = queryMyRules.Select(x => new DropdownValueViewModel
                    {
                        Text = x.NameCN,
                        Value = x.Id.ToString(),
                        ValueInt = x.Id,
                    })
                    .ToList()
                };

                if (myInstance.DropdownItems.IsNotEmptyNull())
                {
                    aggregativeGroupItems.Add(myInstance);
                }
            }

            var viewModel = new AccountingAdjustmentRuleViewModel
            {
                Items = items,
                AggregativeGroupList = aggregativeGroupItems
            };

            viewModel.CopyText_AllParticipantMerchant = SetCopyText(items);
            return viewModel;
        }


        public CRUDResultModel CopyAndCreateNewAdjustmentRule(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.TargetID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.DisplayName).Rule_NotEmptyText();

            int templateRuleID = (int)model.TargetID;
            var queryExist = AccountingAdjustmentRuleRepository.GetById(templateRuleID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var cloneEntity = queryExist.ReturnInjecteObject<AccountingAdjustmentRuleEntity>();

            cloneEntity.NameCN = model.DisplayName;
            cloneEntity.NameEN = model.DisplayName;
            cloneEntity.GameIDs = model.GameIDsText;
            cloneEntity.MerchantIDs = model.MerchantIDsText;
            cloneEntity.AggregativeGroup = model.AggregativeGroup;

            if (model.Begin.HasValue)
            {
                cloneEntity.BeginTime = model.Begin;
            }

            if (model.End.HasValue)
            {
                cloneEntity.EndTime = model.End;
            }

            var newID = AccountingAdjustmentRuleRepository.Create(cloneEntity);

            //若有勾選額外項目
            if (model.IsEnableFlag1)
            {
                var queryTemplate = AccountingAdjustmentRuleGroupAssignRepository.GetByFilter(new QueryAccountingAdjustmentRuleGroupAssignRequestViewModel
                {
                    AccountingAdjustmentRuleIDs = templateRuleID.ToListCollection(),
                });

                var insertEntityItems = queryTemplate.Select(x => new AccountingAdjustmentRuleGroupAssignEntity
                {
                    AccountingGroupID = x.AccountingGroupID,
                    AccountingAdjustmentRuleID = newID
                })
                    .ToList();

                AccountingAdjustmentRuleGroupAssignRepository.CreateMany(insertEntityItems);
            }

            return new CRUDResultModel
            {
                ResultId = newID
            };
        }




        private string SetCopyText(List<AccountingAdjustmentRuleItemViewModel> items)
        {
            string copyText = string.Empty;

            if (items.IsEmptyOrNull())
                return copyText;

            List<EazyKeyAndTextModel> merchantAndDiscount = new List<EazyKeyAndTextModel>();

            foreach (var item in items.Where(x=> !x.IsExpired))
            {
                if (item.MerchantIDs == "*")
                    continue;

                var splitMerchantIDs = item.MerchantIDs.SeperateToListInt();

                if (splitMerchantIDs.IsEmptyOrNull())
                    continue;

                foreach (var merchantID in splitMerchantIDs)
                {
                    merchantAndDiscount.Add(new EazyKeyAndTextModel
                    {
                        Key = merchantID,
                        Text = item.NameCN,
                    });
                }
            }

            List<int> realMerchantIDs = merchantAndDiscount.Select(x => x.Key).ToList();

            var queryHierarchy = MerchantHierarchyRepository.GetJoinInfoByMerchantIDs(realMerchantIDs);

            merchantAndDiscount = merchantAndDiscount
                .OrderBy(x => x.Text.Contains("盤費") ? 1 : 999)
                .ThenBy(x => x.Text)
                .ThenBy(x => x.Key)
                .ToList();

            foreach (var item in merchantAndDiscount)
            {
                int merchantID = item.Key;

                var match = queryHierarchy.FirstOrDefault(x => x.MerchantID == merchantID);

                if(match != null)
                {
                    copyText += match.Currency + "\t" + 
                        match.ParentMerchantName + "\t" +
                        $"{match.MerchantID}｜{match.MerchantName}"  + "\t" +
                        item.Text + Environment.NewLine;
                }
                else
                {
                    var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

                    copyText += queryMerchant.Currency + "\t" +
                        "-" + "\t" +
                        $"{queryMerchant.MerchantID}｜{queryMerchant.Name}" + "\t" +
                        item.Text + Environment.NewLine;
                }
            }

            return copyText;
        }



    }

}
