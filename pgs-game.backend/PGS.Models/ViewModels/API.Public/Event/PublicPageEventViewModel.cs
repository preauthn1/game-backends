using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PublicPageEventViewModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }


        public RefreshIntervalSettingModel IntervalSetting { get; set; }

        public RedEnvelopeEventInfoViewModel RedEnvelope { get; set; }

        public RedEnvelopeEventInfoViewModel LuckyWheel { get; set; }
    }


    public class RefreshIntervalSettingModel
    {
        public RefreshIntervalSettingModel()
        {
            this.RedEnvelope = 5;
            this.LuckyWheel = 5;
        }

        public int RedEnvelope { get; set; }

        public int LuckyWheel { get; set; }
    }


    public class RedEnvelopeEventInfoViewModel
    {

        public int? EventID { get; set; }


        public bool IsEventEnable { get; set; }


        public int RedEnvelopeCount { get; set; }

    }




    public class EventLuckyDrawRequestModel
    {

        public long Ts { get; set; }

        public string SignKey { get; set; }

    }




    public class LuckyDrawRequestModel
    {
        public int PlayerID { get; set; }

        public int EventID { get; set; }

        public int GameID { get; set; }

        public decimal BetAmount { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        public DateTime BetTime { get; set; }

        /// <summary>
        /// 派獎初始狀態
        /// </summary>
        public EventPrizeRecordStatusEnum InitialStatus { get; set; }


        /// <summary>
        /// 是否消耗一張抽獎券 (流程上會處理額外動作)
        /// </summary>
        public bool IsUseDrawTicket { get; set; }

                
        /// <summary>
        /// 消耗指定的活動積分來進行抽獎
        /// </summary>
        public decimal? ConsumeEventPointToDraw { get; set; }


        public int MaxDrawTimes { get; set; }

    }


    public class LuckyDrawResultViewModel
    {
        public int GameID { get; set; }


        public string GameName { get; set; }


        public decimal PrizeRate { get; set; }


        public decimal PrizeAmount { get; set; }


        public long DrawPrizeRecordID { get; set; }


        public decimal ReferenceBetAmount { get; set; }

        /// <summary>
        /// 參考投注金額
        /// </summary>
        public string ReferenceTransactionNo { get; set; }

        /// <summary>
        /// 參考注單時間
        /// </summary>
        public DateTime ReferenceTransactionTime { get; set; }


        public DateTime ReferenceSendTime { get; set; }


        public decimal BeforeAmount { get; set; }


        /// <summary>
        /// 紅包活動。前台刷新錢包餘額的方式是強制觸發agg打getBalance API (透過js)
        /// </summary>
        public decimal AfterAmount { get; set; }


        public string SwCallbackTransactionNo { get; set; }


        public PrizeDrawResultItemDetailModel ItemDetail { get; set; }

    }



    public class PrizeDrawResultItemDetailModel
    {
        public int EventDrawPrizeItemID { get; set; }

        public decimal Amount { get; set; }
    }





}
