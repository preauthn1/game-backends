using Newtonsoft.Json;
using System.Collections.Generic;

namespace game.Entity.ViewModels
{
    public class APLFishBetlistResponseModel
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
        /// 下注时间
        /// </summary>
        public string betTime { get; set; }



        public string startTime { get; set; }



        public string endTime { get; set; }


        /// <summary>
        /// 下注金额
        /// </summary>
        public decimal amt { get; set; }

        /// <summary>
        /// 输赢金额
        /// </summary>
        public decimal payout { get; set; }

        /// <summary>
        /// 退水比
        /// </summary>
        public string rollback { get; set; }

        /// <summary>
        /// 会员下注IP
        /// </summary>
        public string ip { get; set; }

    }

    public class APLFishFishResponseModel
    {
        [JsonConverter(typeof(SingleOrArrayConverter<APLFishBetlistResponseModel>))]
        public List<APLFishBetlistResponseModel> betlist { get; set; }
    }

    public class APLFishBetRecordResponseModel
    {
        public APLFishFishResponseModel fish { get; set; }
    }

}
