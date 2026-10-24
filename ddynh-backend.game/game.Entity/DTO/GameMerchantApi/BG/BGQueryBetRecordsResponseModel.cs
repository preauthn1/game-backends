using System.Collections.Generic;

namespace game.Entity.DTO.GameMerchantApi.BG
{
    public class BGQueryBetRecordsResponseModel : BGBaseResponseModel
    {
        public BGQueryBetRecordsResultResponseModel result { get; set; }
    }



    public class BGQueryBetRecordsStatusResponseModel
    {
        public decimal validAmountTotal { get; set; }
        public decimal aAmountTotal { get; set; }
        public int userCount { get; set; }
        public decimal paymentTotal { get; set; }
        public decimal bAmountTotal_ { get; set; }
        public decimal bAmountTotal { get; set; }
        public decimal validBetTotal { get; set; }
        public decimal paymentTotal_ { get; set; }
    }

    public class BGQueryBetRecordsItemResponseModel
    {
        public object tranId { get; set; }

        /// <summary>
        /// 结算额
        /// </summary>
        public decimal? aAmount { get; set; }


        /// <summary>
        /// 下注额
        /// </summary>
        public decimal bAmount { get; set; }

        /// <summary>
        /// 派彩(输赢)
        /// </summary>
        public decimal payment { get; set; }
        

        /// <summary>
        /// 打码量(有效投注)
        /// </summary>
        public decimal validBet { get; set; }


        /// <summary>
        /// 用户登录ID
        /// </summary>
        public string loginId { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public string orderId { get; set; }


        /// <summary>
        /// 注单状态 (參考 BGBetRecordStatusEnum.cs)
        /// </summary>
        public int orderStatus { get; set; }

        /// <summary>
        /// 玩法ID，唯一标识一种注单类别
        /// </summary>
        public string playId { get; set; }

        /// <summary>
        /// 玩法名称
        /// </summary>
        public string playName { get; set; }

        /// <summary>
        /// 玩法名称(En)
        /// </summary>
        public string playNameEn { get; set; }


        /// <summary>
        /// 用户ID
        /// </summary>
        public int uid { get; set; }

        /// <summary>
        /// 下注时间
        /// </summary>
        public string orderTime { get; set; }


        public string gameName { get; set; }


        /// <summary>
        /// 厅代码
        /// </summary>
        public string sn { get; set; }

        /// <summary>
        /// 模块Id  (參考 BGModuleEnum.cs)
        /// </summary>
        public int moduleId { get; set; }       

        public int? noComm { get; set; }

        public int gameId { get; set; }

        public string gameNameEn { get; set; }

        /// <summary>
        /// 期数ID/局号
        /// </summary>
        public string issueId { get; set; }

     
        public int userId { get; set; }


        /// 下注来源IP
        /// </summary>
        public string fromIp { get; set; }


        /// <summary>
        /// 订单来源，请参见: 终端类别
        /// </summary>
        public int orderFrom { get; set; }


        public string betContent { get; set; }


        /// <summary>
        /// 最后修改时间
        /// </summary>
        public string lastUpdateTime { get; set; }



    }

    public class BGQueryBetRecordsResultResponseModel
    {
        public int total { get; set; }
        public int pageIndex { get; set; }
        public BGQueryBetRecordsStatusResponseModel stats { get; set; }
        public int pageSize { get; set; }
        public object etag { get; set; }
        public int page { get; set; }
        public List<BGQueryBetRecordsItemResponseModel> items { get; set; }
    }



}
