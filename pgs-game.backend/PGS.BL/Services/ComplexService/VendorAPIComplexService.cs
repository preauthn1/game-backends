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
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models;
using target.Models.CultureText;
using target.Models.ViewModels.GameVendor;

namespace target.BL.Services
{
    /// <summary>
    /// 如果有參考多個service 為了避免循環參考 可以寫在這邊
    /// </summary>
    public class VendorAPIComplexService : BaseVendorService, IVendorAPIComplexService
    {
        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }


        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }


        public AgentHierarchyReportSummaryResultViewModel GetRichmondVersionAgentHierarchyReport(AgentHierarchyReportSummaryRequestModel filter)
        {
            filter.RuleFor(x => x.ReportCategoryID).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.ReportApiKey).Rule_NotEmptyText();

            string expectApiKey = [REDACTED]"ReportCategoryID_{filter.ReportCategoryID}");

            if (expectApiKey != filter.ReportApiKey)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel("ReportApiKey"));
            }

            filter.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();

            if (SettingHelper.IsProduction)
            {
                //檢查短時間請求太多次 (避免廠商誤寫了無窮迴圈)
                string apiParameterHash = (filter.ConvertToJsonString() + "GetRichmondVersionAgentHierarchyReport").MD5Hash();
                CheckRequestTooManyTimes(0, apiParameterHash, "GetRichmondVersionAgentHierarchyReport");
            }

            //正式環境才需要檢查range不得太大
            if (!SettingHelper.IsDev)
            {
                filter.RuleFor(x => x.StartTime).Rule_StartTimeEndTimeRange(filter.EndTime, 32);
            }

            var queryTargetMerchants = GetTargetMerchants(filter.ReportCategoryID, filter.Currency);

            var agents = new List<AgentHierarchyReportSummaryEachAgentResultViewModel>();

            foreach(var targerMerchant in queryTargetMerchants)
            {
                string extraWhere = " AND ((GameID >= 1000 AND GameID <= 9999) OR (GameID >= 290000 AND GameID < 299999))";

                var queryReport = PlayerGameDataRepository.GetEachHierarchyMerchantSummaryData(filter.StartTime, filter.EndTime, targerMerchant.MerchantID, extraWhere);

                var convertToViewModels = queryReport
                    .Select(x =>
                    {
                        var convert = new AgentHierarchyReportSummaryDetailItemViewModel
                        {
                            MerchantID = x.MerchantID,
                            MerchantName = x.Name,
                            BetAmount = x.BetAmount,
                            PayoutAmount = x.PayoutAmount,
                            ValidAmount = x.ValidAmount,
                            TotalRound = x.TotalRound,
                            WinLossAmount = x.WinLossAmount,
                            GGR = x.WinLossAmount * -1,
                            TotalBettingPlayerCount = x.PlayerCount,
                        };

                        return convert;
                    })
                    .ToList();

                List<int> merchantIDs = convertToViewModels.Select(x => x.MerchantID).ToList();

                var queryMerchantHierarchyDic = MerchantHierarchyRepository.GetByIds(merchantIDs).ToDictionary(z => z.MerchantID, y => y.Hierarchy);

                foreach (var item in convertToViewModels)
                {
                    item.AgentHierarchyPath = queryMerchantHierarchyDic.GetDictionaryOrDefaultValue(item.MerchantID, "");
                }

                var totalSum = convertToViewModels.CalculateTotalSumMergeToOneObject();
                totalSum.MerchantID = targerMerchant.MerchantID;
                totalSum.MerchantName = targerMerchant.Name;
                totalSum.AgentHierarchyPath = queryMerchantHierarchyDic.GetDictionaryOrDefaultValue(targerMerchant.MerchantID, "");

                agents.Add(new AgentHierarchyReportSummaryEachAgentResultViewModel
                {
                    RootMerchantID = targerMerchant.MerchantID,
                    RootMerchantName = targerMerchant.Name,
                    Currency = targerMerchant.Currency,
                    Items = convertToViewModels,
                    TotalSum = totalSum
                });
            }

            return new AgentHierarchyReportSummaryResultViewModel
            {               
                Agents = agents
            };
        }

        public VendorAPIBackUrlsViewModel GetVendorAPIBackAPIUrls(BackUrlsQueryParameterModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.MerchantSecretKey).Rule_NotEmptyText();

            var queryMerchant = GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            var querySetting = OneToManyCustomizedSettingItemRepository
                .GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.VendorAPIUrlDoamin_BackupUrls);

            return new VendorAPIBackUrlsViewModel
            {
                Urls = querySetting.Select(z=> z.SettingValue).ToList(),
            };
        }


        private List<MerchantEntity> GetTargetMerchants(int tagID, string currency)
        {
            var queryItems = GeneralTagMappingRepository.GetByFilter(new QueryGeneralTagMappingRequestViewModel
            {
                TagIDs = tagID.ToListCollection()
            });

            if (queryItems.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted);

            List<int> merchantIDs = queryItems.Select(z => z.TargetID).ToList();

            var queryMerchants = MerchantRepository.GetByIds(merchantIDs);

            var matchMerchants = queryMerchants;

            if(!string.IsNullOrWhiteSpace(currency))
            {
                matchMerchants = queryMerchants
                    .Where(x => x.Currency == currency)
                    .ToList();
            }

            if (matchMerchants.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            return matchMerchants;
        }



    }

}
