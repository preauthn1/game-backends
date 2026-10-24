using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class HappyFishingBetRecordApiResponse
    {
        public int code { get; set; }
        public int count { get; set; }
        public List<HappyFishingBetRecordApiItemModel> data { get; set; }
        public string msg { get; set; }
    }


    public class HappyFishingBetRecordApiItemModel
    {
        public int bet_total { get; set; }
        public string channel { get; set; }
        public int profit_total { get; set; }
        public string room_id { get; set; }
        public string roundbegin { get; set; }
        public string roundend { get; set; }
        public int roundid { get; set; }
        public string uid { get; set; }
        public int win_total { get; set; }
    }



    public class HappyFishingTransferRecordResponseItemModel
    {
        public string uin { get; set; }
        public string platformId { get; set; }
        public int orderId { get; set; }
        public string transId { get; set; }

        /// <summary>
        /// 兌出(結算) = -1 
        /// 兌入(投注) = 1
        /// </summary>
        public int direct { get; set; }

        public int coin { get; set; }

        /// <summary>
        /// 會是 1000倍?
        /// </summary>
        public int fishCoin { get; set; }

        public string platformCoin { get; set; }
        public DateTime opTime { get; set; }
        public int result { get; set; }
        public string channel { get; set; }
        public int source { get; set; }
        public int step { get; set; }
    }

    public class HappyFishingTransferRecordPagingResponseModel
    {
        public int code { get; set; }
        public List<HappyFishingTransferRecordResponseItemModel> data { get; set; }
        public int count { get; set; }
        public string proc { get; set; }
        public string proc2 { get; set; }
    }

    public class HappyFishingTransferRequestParameter
    {
        public DateTime BeginUTC { get; set; }

        public DateTime EndUTC { get; set; }

        public string AuthToken { get; set; }

        public int SplitMinutes { get; set; }

        public string UID { get; set; }


        public bool IsTo_595959 { get; set; }


    }


    public class HappyFishingRoomFlowAmountResponseItem
    {
        public string platform { get; set; }
        public int logid { get; set; }
        public int uin { get; set; }
        public string name { get; set; }
        public string roomName { get; set; }
        public int putScore { get; set; }
        public int getScore { get; set; }
        public int gainCoin { get; set; }
        public int costCoin { get; set; }
        public string enterTime { get; set; }
        public string leaveTime { get; set; }
        public int enterCoin { get; set; }
        public int ticket { get; set; }
        public int gainTicket { get; set; }
        public decimal costTicket { get; set; }
        public decimal gainMoney { get; set; }
        public decimal gain { get; set; }
        public decimal extraCoin { get; set; }
        public int taxScore { get; set; }
        public int status { get; set; }
        public string channel { get; set; }
        public string fishIds { get; set; }
        public int playId { get; set; }
    }

    public class HappyFishingRoomFlowAmountResponse
    {
        public int code { get; set; }
        public List<HappyFishingRoomFlowAmountResponseItem> data { get; set; }
        public int count { get; set; }
        public string proc { get; set; }
        public string proc2 { get; set; }
    }




    public class TempRoomFlowAmountEntity
    {
        public int Id { get; set; }

        public DateTime StatDate { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public string UID { get; set; }

        public decimal RoomInAmount { get; set; }

        public decimal ProfitAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        public decimal FreeAmount { get; set; }

        public decimal ValidAmount { get; set; }

    }







}
