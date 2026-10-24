using Newtonsoft.Json;
using System.Collections.Generic;

namespace game.Entity.ViewModels
{
    public class APLGameMerchantApiQueryBetRecordModel
    {
        public APLLBetRecordItemModel lottory { get; set; }

        public APLLBetRecordItemModel slot { get; set; }

        public APLFishBetRecordResponseModel fish { get; set; }
    }



    public class APLBetRecordBetListItemModel
    {
        /// <summary>
        /// 币别
        /// </summary>
        public string curId { get; set; }

        /// <summary>
        ///  游戏编号
        /// </summary>
        public string gameId { get; set; }

        /// <summary>
        /// 游戏类别
        /// </summary>
        public string gameType { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string gameName { get; set; }

        /// <summary>
        /// 会员账号
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 注单编号
        /// </summary>
        public string betId { get; set; }

        /// <summary>
        /// 期数编号
        /// </summary>
        public string eventId { get; set; }

        /// <summary>
        /// 注单状态: 0:未开奖, 1:已开奖, 3:取消单
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 下注时间
        /// </summary>
        public string betTime { get; set; }

        /// <summary>
        /// 开奖结果
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// 开奖号码 PS:代码参考 游戏代码表
        /// </summary>
        public string resultCode { get; set; }

        /// <summary>
        /// 结算时间
        /// </summary>
        public string billTime { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal amt { get; set; }

        /// <summary>
        /// 输赢金额
        /// </summary>
        public decimal payout { get; set; }

        /// <summary>
        /// 玩法代码 PS:代码参考 游戏代码表
        /// </summary>
        public string playId { get; set; }

        /// <summary>
        /// 下注号码
        /// </summary>
        public string number { get; set; }

        /// <summary>
        /// 下注内容
        /// </summary>
        public string selection { get; set; }

        /// <summary>
        /// 赔率
        /// </summary>
        public string odds { get; set; }

        /// <summary>
        /// 退水比
        /// </summary>
        public string rollback { get; set; }

        /// <summary>
        /// 会员下注IP
        /// </summary>
        public string ip { get; set; }

        /// <summary>
        /// 会员下注装置
        /// </summary>
        public object device { get; set; }
    }




    public class TotalGoldTotalWagerListModel
    {
        public string gametype { get; set; }


        /// <summary>
        /// 下注笔数
        /// </summary>
        public int num { get; set; }

        /// <summary>
        /// 总下注额
        /// </summary>
        public decimal totalgold { get; set; }

        /// <summary>
        /// 总有效投注额
        /// </summary>
        public decimal totalrealgold { get; set; }

        /// <summary>
        /// 总派彩
        /// </summary>
        public decimal totalwingold { get; set; }
    }

    public class TotalGoldReportModelt
    {
        public List<TotalGoldTotalWagerListModel> TotalWagerList { get; set; }
    }



    public class APLLBetRecordItemModel
    {
        [JsonConverter(typeof(SingleOrArrayConverter<APLBetRecordBetListItemModel>))]
        public List<APLBetRecordBetListItemModel> betlist { get; set; }
    }



}







