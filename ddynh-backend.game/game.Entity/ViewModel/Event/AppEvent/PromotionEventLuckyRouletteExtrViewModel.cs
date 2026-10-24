using System.Collections.Generic;

namespace game.Entity.ViewModels
{

    /// <summary>
    /// 轉盤特有的欄位
    /// </summary>
    public class PromotionEventLuckyRouletteExtrViewModel
    {

        public int TicketCount { get; set; }

        /// <summary>
        /// 可能參加規則是後台自由填寫的
        /// </summary>
        public string EventMsg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TipsMsg { get; set; }


        public List<PromotionEventLuckyRouletteBonusItemViewModel> BonusItems { get; set; }

    }



    public class PromotionEventLuckyRouletteBonusItemViewModel
    {

        public int BonusID { get; set; }


        public string BonusName { get; set; }

        public decimal Amount { get; set; }

        /// <summary>
        /// 大概是參考 EventBonusTypeEnum
        /// </summary>
        public int BonusType { get; set; }


        /// <summary>
        /// 圖片是要存在app裡面 還是網址url ?
        /// </summary>
        public string ImageUrl { get; set; }
    }


    public class PromotionEventLuckyRoulettDrawResultModel
    {
        /// <summary>
        /// 抽獎結果
        /// </summary>
        public bool IsSuccess { get; set; }

        public string ResultMsg { get; set; }

        /// <summary>
        /// 獎品ID
        /// </summary>
        public int BonusID { get; set; }

        /// <summary>
        /// 剩餘彩票數
        /// </summary>
        public int LotteryRemaining { get; set; }

    }



}
