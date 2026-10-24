namespace target.Models.Enums
{
    public enum SpecialMerchantFilterType
    {
        Recent_14Days = 1007,
        Recent_1Month = 1009,
        Recent3Month = 1011,

        IDR_And_KIDR_Production = 2222,
        USD_Region_Production = 2223,

        RootAgentOnly = 3085,
        ExcludeLayer3Agent = 3091,
        SearchLayer2AgentOnly = 3092,
        NotAgentOnly = 3099,


        RichmondMerchantOnly = 3201,
        IONSlotMerchantOnly = 3202,

        //禁止定義超過10000以上的編號 會跟標籤篩選衝突
        //禁止定義超過10000以上的編號 會跟標籤篩選衝突
        //禁止定義超過10000以上的編號 會跟標籤篩選衝突
        //禁止定義超過10000以上的編號 會跟標籤篩選衝突
        //禁止定義超過10000以上的編號 會跟標籤篩選衝突
        //禁止定義超過10000以上的編號 會跟標籤篩選衝突

    }

    public enum SpecialFilterTypeOfIPAddressTemplate
    {

        AssignMerchantOnly = 1701,      
        
    }

    public enum SpecialFilterTypeOfIMChannels
    {

        AssignProductionMerchantOnly = 5511,

    }


    public enum SpecialFilterTypeOfApiReqeustLog
    {

        /// <summary>
        /// 透過所有可能欄位 搜尋符合merchantID的紀錄
        /// </summary>
        VendorAPI_SearchAllColumnByMerchantID = 3377,

        ResponseTime_Large_Than_3 = 5103,

        ResponseTime_Large_Than_10 = 5110,

        //ErrorLogAPI_SearchThirdBetRecordNo_Recent_14_Dayss = 4111,

    }



    public enum SpecialFilterTypeOfSwApiRequestLog
    {
        /// <summary>
        /// 請求時間大於 3秒
        /// </summary>
        RequestTime_LargeThan_3_Seconds = 2003,

        /// <summary>
        /// 請求時間大於 3秒
        /// </summary>
        RequestTime_LargeThan_10_Seconds = 2010,


        /// <summary>
        /// 有ErrorCode 或 非200
        /// </summary>
        ErrorCode_Or_Not200Response = 4888,

    }

    public enum SpecialFilterTypeOfUsers
    {
       
        UseDefaultPassword = [REDACTED],

        LastLogin_30Days = 6222,

        AssignDisableMerchant = 6555,

        HasRiskScoreTag = 6777,
    }

}
