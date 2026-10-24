using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{
    public class EGamingGetWalletBalanceReponseModel
    {
        public string username { get; set; }

        public decimal money { get; set; }
    }



    public class EGamingGameMerchantApiGameLaunchUrlResponseModel : BaseEGGameApiRecordResponseModel
    {
        public string h5_link { get; set; }

    }



    public class EGamingGameMerchantApiTransferRecordResponseModel : BaseEGGameApiRecordResponseModel
    {
     
        public string external_transaction_id { get; set; }
    }


    public class EGamingGameMerchantApiWalletTransferResponseModel : BaseEGGameApiRecordResponseModel
    {
      
        public decimal money { get; set; }

        public string external_transaction_id { get; set; }

    }


    public class EGamingGameMerchantApiCommonResponseModel<T> : BaseEGGameApiRecordResponseModel
    {

        public T results { get; set; }
    }


    public class EGamingGameMerchantApiQueryBetRecordResponseModel : BaseEGGameApiRecordResponseModel
    {

        public int count { get; set; }

        public List<EGamingGameMerchantApiBetRecordItemodel> list { get; set; }
    }



    public class EGamingGameMerchantApiQueryReportSummaryResponseModel : BaseEGGameApiRecordResponseModel
    {

        /// <summary>
        /// 总下注额(实际值的100倍)  
        /// </summary>
        public long bet { get; set; }

        /// <summary>
        /// 总派彩额(实际值的100倍) 
        /// </summary>
        public long payout { get; set; }

        /// <summary>
        /// 总盈余(实际值的100倍) 
        /// </summary>
        public long surplus { get; set; }


        /// <summary>
        /// 下注总笔数（一个玩家在一局里面下注算一笔） 
        /// </summary>

        public int bet_count { get; set; }


    }



    public class EGamingGameMerchantApiBetRecordItemodel 
    {

        /// <summary>
        /// 游戏名称，见真人游戏type参数说明 Live Casino Param Type Description bet
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// 本条下注记录的唯一 Id bet_id,Unique 
        /// </summary>
        public string bet_id { get; set; }


        public string username { get; set; }


        public string ip { get; set; }

        /// <summary>
        /// 牌局号 
        /// </summary>
        public string round_id { get; set; }

        /// <summary>
        /// 开始时间 
        /// </summary>
        public long start_bet_time { get; set; }

        /// <summary>
        /// 停止下注时间 
        /// </summary>
        public long stop_bet_time { get; set; }

        /// <summary>
        /// 派彩时间 
        /// </summary>
        public long payout_time { get; set; }

        /// <summary>
        /// 撤单时间 
        /// </summary>
        public long repeal_time { get; set; }

        /// <summary>
        /// 派彩(返回金额为实际金额 100 倍) 
        /// </summary>
        public long payout { get; set; }

        /// <summary>
        /// 盈余(返回金额为实际金额 100 倍）
        /// </summary>
        public long surplus { get; set; }

        /// <summary>
        /// 用户的有效下注(返回金额为实际金额 100 倍) 
        /// </summary>
        public long valid_bet { get; set; }

        /// <summary>
        /// 开牌结果，player:闲 player_pair:闲对 tie:和 banker:庄 banker_pair:庄对 bet
        /// </summary>
        public IEnumerable<string> round_result { get; set; }

        /// <summary>
        /// 下注内容，player:闲 player_pair:闲对 tie:和 banker:庄 banker_pair:庄对 bet 
        /// </summary>
        public string bet_on { get; set; }

        /// <summary>
        /// 下注金额(返回金额为实际金额 100 倍） 
        /// </summary>
        public long bet { get; set; }

        /// <summary>
        /// 牌型，见百家乐牌型说明 refer to Baccarat Card Description 
        /// </summary>
        public object cards { get; set; }


    }





    public class EGamingGameMerchantApiQueryTransferRecordResponseModel : BaseEGGameApiRecordResponseModel
    {        

        public string external_transaction_id { get; set; }

    }


    public class BaseEGGameApiRecordResponseModel
    {
        public int code { get; set; }

        public string msg { get; set; }

    }




    public class EGamingGameMerchantApiQueryDailyReportSummaryParameterModel
    {
        public int PlatformID { get; set; }

        public DateTime BeginDate_8TimeZone { get; set; }

        public DateTime EndDate_8TimeZone { get; set; }

    }



}
