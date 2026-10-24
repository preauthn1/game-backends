using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace game.Entity.DTO.GameMerchantApi.BG
{
    public class SumStatResponseModel
    {
        public int orderCount { get; set; }
        public int userCount { get; set; }
        public decimal betAmount { get; set; }
        public decimal payOutAmount { get; set; }
        public decimal validAmount { get; set; }
        public string jackpot { get; set; }
        public int extend { get; set; }
    }
    public class SubStatResponseModel
    {
        public int orderCount { get; set; }
        public int userCount { get; set; }
        public decimal betAmount { get; set; }
        public decimal payOutAmount { get; set; }
        public decimal validAmount { get; set; }
        public string jackpot { get; set; }
        public int extend { get; set; }
    }
    public class StatsResponseModel
    {
        public SumStatResponseModel sumStat { get; set; }
        public SubStatResponseModel subStat { get; set; }
    }
    public class BGFishBetItemsResponseModel
    {
        public string sn { get; set; }
        public long userId { get; set; }
        public string loginId { get; set; }
        public string issueId { get; set; }

        /// <summary>
        /// 注单号
        /// </summary>
        public string betId { get; set; }

        public string gameBalance { get; set; }

        /// <summary>
        /// 发炮数量
        /// </summary>
        public int fireCount { get; set; }

        /// <summary>
        /// 下注额
        /// </summary>
        public decimal betAmount { get; set; }

        /// <summary>
        /// 有效投注
        /// </summary>
        public decimal validAmount { get; set; }

        /// <summary>
        /// 结算金额
        /// </summary>
        public decimal calcAmount { get; set; }

        /// <summary>
        /// 派彩(输赢)
        /// </summary>
        public decimal payout { get; set; }

        /// <summary>
        /// 下注时间(UTC-4)
        /// </summary>
        public string orderTime { get; set; }
        public string orderFrom { get; set; }

        /// <summary>
        /// JP中奖
        /// </summary>
        public string jackpot { get; set; }

        /// <summary>
        /// JP预抽
        /// </summary>
        public string extend { get; set; }

        /// <summary>
        /// 奖池类型
        /// </summary>
        public int jackpotType { get; set; }
    }
    public class ResultResponseModel
    {
        public int total { get; set; }
        public int pageIndex { get; set; }
        public StatsResponseModel stats { get; set; }
        public int pageSize { get; set; }
        public object etag { get; set; }
        public int page { get; set; }
        public List<BGFishBetItemsResponseModel> items { get; set; }
    }



    public class BGQueryFishBetRecordsResponseModel : BGBaseResponseModel
    {
        public ResultResponseModel result { get; set; }
    }



}
