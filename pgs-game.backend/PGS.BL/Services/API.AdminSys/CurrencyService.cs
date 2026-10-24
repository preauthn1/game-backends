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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using target.Utility.Extensions.Web;
using target.Utility;
using target.Utility.Utility;

namespace target.BL.Services
{

    public class CurrencyService : BaseAdminSysService, ICurrencyService
    {

        public CurrencyRepository CurrencyRepository { get; set; }


        public CurrencyManagementViewModel GetAllCurrencyList(int userID)
        {
            var queryAll = CurrencyRepository.GetAll();

            //檢查一些資料異常
            foreach(var item in queryAll)
            {
                if(item.Currency != item.Currency.Trim())
                {
                    SendMessageToMultipleChannel($"貨幣:{item.Currency} 有空白字元");
                }
            }

            List<CurrencyItemViewModel> items = new List<CurrencyItemViewModel>();

            var now = DateTimeHelper.GetCurrentTime();

            var relativeCurrencyList = GetCurrencyMappingRelativeCurrency();

            List<string> hideCurrency = relativeCurrencyList.Select(x => x.Value).Select(z => z.Currency).Distinct().ToList();

            items = queryAll
                .Where(x=> !hideCurrency.Contains(x.Currency)) //KVND、KIDR 這些隱藏
                .Select(x =>
                {
                    var instance = x.ReturnInjecteObject<CurrencyItemViewModel>();
                    instance.LastUpdateTime = x.LastUpdateTime ?? new DateTime(2023, 1, 1);
                    instance.CurrencyText = MultiLanguageTextHelper.GetStringEnumsCultureText<CurrencyEnum>(instance.Currency);

                    var totalMinutes = (int)(now - instance.LastUpdateTime).TotalMinutes;
                    instance.LastUpdateTimeDiffText = DateTimeHelper.GetDiffDisplayTime(totalMinutes, isIncludeParentheses: false, "前");

                    if (totalMinutes >= 60 * 24 * 60)
                    {
                        instance.IsDiscardManyDays = true;
                    }

                    return instance;
                })
                .OrderBy(x =>  x.IsHot ? 0: 999)
                .ThenBy(x => (now - x.LastUpdateTime).TotalDays < 60 ? 1 : 999)
                .ThenBy(x => x.Currency)
                .ToList();

            SetExtraProperty(items);
            return new CurrencyManagementViewModel
            {
                Items = items
            };
        }

        public CRUDResultModel Update(int userID, CurrencyUdpateRequestModel model)
        {
            model.RuleFor(x => x.Currency).Rule_NotEmptyText();
            model.RuleFor(x => x.ExchangeCurrency).Rule_NotEmptyText();
            model.RuleFor(x => x.ExchangeRate).Rule_NotZeroNotMinus();

            bool isEditUSDT = false;
            bool isEditTWD = false;

            switch(model.ExchangeCurrency)
            {
                case "USDT":
                    isEditUSDT = true;
                    break;

                case "TWD":
                    isEditTWD = true;
                    break;

                default:
                    throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"Currency:{model.ExchangeCurrency}"));
            }

            var query = CurrencyRepository.GetById(model.Currency);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            decimal originalRate = 0;
            decimal afterlRate = 0;

            if(isEditUSDT)
            {
                originalRate = query.ExchangeRateToUSDT;
                query.ExchangeRateToUSDT = model.ExchangeRate;
            }
            else if(isEditTWD)
            {
                originalRate = query.ExchangeRateToTWD;
                query.ExchangeRateToTWD = model.ExchangeRate;
            }

            afterlRate = model.ExchangeRate;

            decimal diffRate = 0;

            if (originalRate != 0)
            {
                diffRate = (afterlRate - originalRate).ToAbs() / originalRate * 100;
            }

            string debugInfo = $"{originalRate} => {afterlRate} ({Math.Round(diffRate, 2)}%)";

            query.LastUpdateTime = DateTimeHelper.GetCurrentTime();

            CurrencyRepository.Update(query);

            CurrencyEntity relativeCurrency = null;
            decimal relativeExchangeRateAdjust = 1;

            //確認是否這個貨幣有額外對應的關聯貨幣 (e.g. KIDR KVND...)
            var relativeCurrencyList = GetCurrencyMappingRelativeCurrency();

            if(relativeCurrencyList.ContainsKey(model.Currency))
            {
                relativeCurrency = CurrencyRepository.GetById(relativeCurrencyList[model.Currency].Currency);
                relativeExchangeRateAdjust = relativeCurrencyList[model.Currency].ExchangeRateDiff;
            }

            //同步更新關聯貨幣的匯率 (e.g. KIDR KVND...)
            if (relativeCurrency != null)
            {
                if (isEditUSDT)
                {
                    relativeCurrency.ExchangeRateToUSDT = query.ExchangeRateToUSDT * relativeExchangeRateAdjust;
                }
                else if (isEditTWD)
                {
                    relativeCurrency.ExchangeRateToTWD = query.ExchangeRateToTWD * relativeExchangeRateAdjust;
                }

                relativeCurrency.LastUpdateTime = DateTimeHelper.GetCurrentTime();
                CurrencyRepository.Update(relativeCurrency);
            }

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateCurrencyRate, 0, debugInfo);
            log.TargetKey = query.Currency;
            log.Memo = $"ExchangeTo{model.ExchangeCurrency}";

            WriteOperateLog(log);

            if (diffRate > 20 && SettingHelper.IsProduction)
            {
                SendMessageToMultipleChannel($"{query.Currency} 匯率被修正。差距{Math.Round(diffRate, 2)}%。請確認原因");
            }

            return new CRUDResultModel
            {
                ResultTime = query.LastUpdateTime,
                ResultNumber = originalRate,
            };
        }


        public void UpdateCurrencyRateByAPI(int userID)
        {
            var days60 = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60);

            var queryAllCurrency = CurrencyRepository.GetAll()
                .Where(x => !x.IsHot)
                .Where(x=> x.LastUpdateTime > days60)
                .ToList();

            var referCurrenctMapping = GetCurrencyMappingRelativeCurrency()
                .Select(x => x.Value.Currency)
                .ToList();

            Dictionary<string, decimal> exchangeRate_toUSD = new Dictionary<string, decimal>();
            Dictionary<string, decimal> exchangeRate_toTWD = new Dictionary<string, decimal>();

            foreach(var eachCurrency in queryAllCurrency)
            {
                string myCurrency = eachCurrency.Currency;

                if (myCurrency == "BETA" || referCurrenctMapping.Any(z=> z == myCurrency))
                    continue;

                var apiResult = APIRequestHelper.RequestApiResult<ExchangerateApiResponseModel>(new RequestParameters
                {
                    BaseUrl = "https://example.invalid",
                    ApiUrl = $"v6/f64a0153ebca13c38324c46c/latest/{myCurrency}",
                    Method = HttpMethodType.HttpGet
                });

                var rate_TWD = apiResult.ResultObject.conversion_rates["TWD"];
                var rate_USD = apiResult.ResultObject.conversion_rates["USD"];

                //台幣匯率短期內不會有用處了 (AGG帳單改用USD匯率)
                //exchangeRate_toTWD.Add(myCurrency, rate_TWD);
                exchangeRate_toUSD.Add(myCurrency, rate_USD);
            }

            foreach(var item in exchangeRate_toTWD)
            {
                Update(userID, new CurrencyUdpateRequestModel
                {
                    Currency = item.Key,
                    ExchangeRate = item.Value,
                    ExchangeCurrency = "TWD",
                });
            }

            foreach (var item in exchangeRate_toUSD)
            {
                Update(userID, new CurrencyUdpateRequestModel
                {
                    Currency = item.Key,
                    ExchangeRate = item.Value,
                    ExchangeCurrency = "USDT",
                });
            }

            return;
        }


        private Dictionary<string, CurrencyRelativeModel> GetCurrencyMappingRelativeCurrency()
        {
            var dic = new Dictionary<string, CurrencyRelativeModel>();
            dic.Add("MYR", new CurrencyRelativeModel("MYR100", 0.01M));
            dic.Add("VND", new CurrencyRelativeModel("KVND", 1000));
            dic.Add("IDR", new CurrencyRelativeModel("KIDR", 1000));
            dic.Add("COP", new CurrencyRelativeModel("KCOP", 1000));
            dic.Add("MMK", new CurrencyRelativeModel("KMMK", 1000));

            return dic;
        }

        private void SetExtraProperty(List<CurrencyItemViewModel> items)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            foreach (var item in items)
            {

            }
        }


    }
}
