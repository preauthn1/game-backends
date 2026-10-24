using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class BetRecordBasicInfo
    {
        public string BetRecordNoPGS { get; set; }

        public int GameVendorID { get; set; }

        public string GameName { get; set; }

        public string PlayerName { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public DateTime CreateTime { get; set; }

        public string VideoUrl { get; set; }

        public bool IsRawHtmlLayout { get; set; }


        /// <summary>
        /// 請參考 VideoFormatTypeEnum
        /// </summary>
        public int VideoFormat { get; set; }
    }



    public class RoundInfoItemModel
    {
        public string Category { get; set; }

        public List<string> Images { get; set; }

        public string MemoInfo { get; set; }

        public bool IsFullWidthDisplay { get; set; }

    }



    public class AGGLiveBackOfficeRecordDetailApiResponse
    {
        /// <summary>
        /// 若成功會回 1000
        /// </summary>
        public int code { get; set; }

        public string status { get; set; }

        public AGGLiveBackOfficeRecordDetailApiResult result { get; set; }
    }


    public class AGGLiveBackOfficeRecordDetailApiBetInfo
    {
        public List<AGGLiveBackOfficeRecordDetailApiBetList> bet_list { get; set; }
    }

    public class AGGLiveBackOfficeRecordDetailApiBetList
    {
        /// <summary>
        /// 這個欄位是pgs自訂的
        /// </summary>
        public List<string> bet_type_images { get; set; }

        /// <summary>
        /// 這個欄位是pgs自訂的
        /// </summary>
        public bool is_image_display { get; set; }

        public string bet_type { get; set; }
        public decimal bet_amount { get; set; }
        public decimal win_amount { get; set; }
        public decimal tax_amount { get; set; }
        public string no_commission { get; set; }

    }

    public class AGGLiveBackOfficeRecordDetailApiDetail
    {
        public List<AGGLiveBackOfficeRecordDetailApiRoundInfo> round_info { get; set; }
        public AGGLiveBackOfficeRecordDetailApiBetInfo bet_info { get; set; }
    }

    public class AGGLiveBackOfficeRecordDetailApiResult
    {
        public AGGLiveBackOfficeRecordDetailApiDetail detail { get; set; }
        public string round { get; set; }
    }

    public class AGGLiveBackOfficeRecordDetailApiRoundInfo
    {
        public string ball { get; set; }
        public List<string> player1_card { get; set; }
        public List<string> player2_card { get; set; }
        public List<string> player3_card { get; set; }
        public object player_card { get; set; }
        public List<string> banker_card { get; set; }
        public List<string> dragon_card { get; set; }
        public List<string> tiger_card { get; set; }
        

        public List<string> dice { get; set; }
        public List<string> win_list { get; set; }
        public List<RoundExtraInfo> super_number { get; set; }
    }


    public class RoundExtraInfo
    {
        public string description { get; set; }
        public string payout { get; set; }
        public string super_odds { get; set; }
    }


    public class AGGLiveBackOfficeLoginApiResponse
    {
        public int code { get; set; }
        public AGGLiveBackOfficeLoginApiResponseDetailModel result { get; set; }
    }


    public class AGGLiveBackOfficeLoginApiResponseDetailModel
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public int operator_id { get; set; }
        public int role_id { get; set; }
        public string token { get; set; }
        public string time_zone { get; set; }
    }


}
