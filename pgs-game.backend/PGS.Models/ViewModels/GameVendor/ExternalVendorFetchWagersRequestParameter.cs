using target.Models.Table;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
    public class ExternalVendorFetchWagersRequestParameter
    {
        public int VendorConfigID { get; set; }


        /// <summary>
        /// 非必填、若有需要指定特定遊戲
        /// </summary>
        public string GameCode { get; set; }


        /// <summary>
        /// 請統一傳UTC過來，在根據遊戲商不同 自行轉成對方接受的時區
        /// 若查詢 2020-01-01 當日的報表，StartTimeUTC 會帶入 2020-01-01 00:00:00
        /// </summary>
        public DateTime StartTimeUTC { get; set; }


        /// <summary>
        /// 若查詢 2020-01-01 當日的報表，EndTimeUTC 會帶入 2020-01-02 00:00:00
        /// 請注意各遊戲商API接收的endTime格式要再自行做轉換 (e.g. 起始結束 要同一日)
        /// </summary>
        public DateTime EndTimeUTC { get; set; }


        public int Page { get; set; }


        /// <summary>
        /// (非必要-特殊使用) 指定特定gameIDs
        /// </summary>
        public List<int> Extra_GameIDs { get; set; }


        /// <summary>
        /// (非必要-特殊使用) 查詢特定單號
        /// </summary>
        public string Extra_SearchWagerID { get; set; }


    }


    public class ExternalVendorFetchWagersResult
    {
        public string ResponseText { get; set; }        

        /// <summary>
        /// 返回Model一定要填寫這個欄位
        /// </summary>
        public int PageSize { get; set; }


        public List<ThirdPartyFetchRecordBriefEntity> ConvertInsertEntity { get; set; }

        public JiliBetRecordsApiResponseModel Jili { get; set; }

        public MarioFetchWagersApiModel Mario { get; set; }

        public FunkyFetchWagersApiModel Funky { get; set; }

    }



}
