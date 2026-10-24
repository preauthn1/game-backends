using FW.Core.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{

    public class PromotionEventLotteryResultModel
    {

        public int EventID { get; set; }

        public string EventMsg { get; set; }

        /// <summary>
        /// 剩餘數量
        /// </summary>
        public int LotteryRemaining { get; set; }


    }
}
