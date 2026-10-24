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
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Entity.Game;
using System.Threading;
using target.Utility;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class HappyFishingJobService : BaseJobService, IHappyFishingJobService
    {     
        public BetRecordFishRepository BetRecordFishRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }


        public string CheckPendingBetRecord()
        {
            var now = DateTimeHelper.GetCurrentTime();

            var queryPendingRecords = BetRecordFishRepository.GetByFilter(new QueryBetRecordGeneralRequestViewModel
            {
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                CreateTimeBegin = now.AddDays(-14),
                CreateTimeEnd = now.AddHours(-2)
            });

            if (queryPendingRecords.IsEmptyOrNull())
                return "無注單";

            var fishService = new CrazyFishingIntegrationService();

            foreach(var item in queryPendingRecords)
            {
                try
                {
                    var apiResult = fishService.GetFishingTransferRecord(new HappyFishingTransferRecordApiRequestParameter
                    {
                        MerchantID = item.MerchantID,
                        TransactionId = item.BetRecordNo,
                    });

                    string defaultMsg = $"捕魚單號: `{item.BetRecordNo}` 商戶號:{item.MerchantID} 兌入兌出Api查無紀錄，需手動去後台查詢";

                    if (apiResult.ResultObject != null &&
                       apiResult.ResultObject.data.IsNotEmptyNull() &&
                       apiResult.ResultObject.data.Count() == 2)
                    {
                        if (apiResult.ResultObject.data.Count() == 2)
                        {
                            var placeBet = apiResult.ResultObject.data.FirstOrDefault(z => z.direct == 1);
                            var settleBet = apiResult.ResultObject.data.FirstOrDefault(z => z.direct == -1);

                            if (placeBet == null || settleBet == null)
                            {
                                NotifyToMultipleJobChannel($"捕魚單號: `{item.BetRecordNo}` api返回的物件異常");
                                continue;
                            }

                            decimal rate = placeBet.fishCoin / item.BetAmount;
                            rate = rate == 0 ? 1 : rate;

                            string settleMsg = $"捕魚單號: `{item.BetRecordNo}` 投注金額:{item.BetAmount} 商戶號:{item.MerchantID} 超過時間未結算、API有兌入兌出紀錄 {Environment.NewLine}"
                                + $"投注金額:{placeBet.fishCoin} ({placeBet.fishCoin / rate})、派彩金額:{settleBet.fishCoin} ({settleBet.fishCoin / rate})  ps.可能有 `{rate}倍` 的比例換算";

                            NotifyToMultipleJobChannel(settleMsg);
                        }
                        else if(apiResult.ResultObject.data.Count() == 1)
                        {
                            string msg = $"捕魚單號: `{item.BetRecordNo}` , 投注金額:{item.BetAmount} , 只有兌入紀錄 沒有兌出紀錄";
                            NotifyToMultipleJobChannel(msg);
                        }
                        else
                        {
                            NotifyToMultipleJobChannel(defaultMsg);
                        }
                    }
                    else
                    {
                        NotifyToMultipleJobChannel(defaultMsg);
                    }
                }
                catch(Exception ex)
                {
                    NotifyToMultipleJobChannel($"捕魚單號: `{item.BetRecordNo}` 程式拋錯誤" + ex.Message);
                }
            }

            return $"有{queryPendingRecords.Count()}未結算注單";
        }

    }
}
