using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventProcessTempInfo
    {
        public EventProcessTempInfo()
        {
            this.ValidateResultsForDebug = new List<EventRuleCheckResultDebugInfoModel>();
        }


        public PlayerEntity Player { get; set; }

        public List<EventEntity> Events { get; set; }

        public List<EventRuleCheckResultDebugInfoModel> ValidateResultsForDebug { get; set; }

        public int GameID { get; set; }

        /// <summary>
        /// 每日型任務 判定的日期基準
        /// </summary>
        public DateTime TargetDateOfDailyQuest { get; set; }

        public bool IsDebugModeSkipPrize { get; set; }

        public bool IsSingleWallet { get; set; }

        public string DebugMethodName { get; set; }


        public ExtraEventProcessParameter ExtraParameter { get; set; }

        public EventCounterValuePlusSPResultModel EventCounterOfGame { get; set; }

        public EventCounterValuePlusSPResultModel EventCounterOfAllInOne { get; set; }


        /// <summary>
        /// 因為有迴圈處理的機制、效能考量。會預先透過另一隻method 查出所有既有資料
        /// </summary>
        public bool IsPreQueryEventCounter { get; set; }


        /// <summary>
        /// 預先查詢好的 累計積分資訊
        /// </summary>
        public List<EventBetCounterEntity> PreQueryEventCounterList { get; set; }

    }


    public class EventProcessTempPlayerInfo
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int MerchantID { get; set; }
    }






}
