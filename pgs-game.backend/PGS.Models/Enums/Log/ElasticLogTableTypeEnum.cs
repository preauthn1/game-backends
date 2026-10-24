using System.ComponentModel;

namespace target.Models.Enums
{

    /// <summary>
    /// 基本上大部分的儲存table 都會跟enum一樣名稱 (SQL、Elasticsearch)
    /// 所以麻煩不要亂改enum名稱
    /// 若有特別不同的則會在 TableNameConvertHelper 定義
    /// 有幾個 if else 判斷是寫死在 TableNameConvertHelper 若新增項目要注意
    /// </summary>
    public enum ElasticLogTableTypeEnum
    {
        None = 0,

        // 正常Log
        // 正常Log
        // 正常Log
        ApiRequestLogAdminSysAPI = 1001,
        ApiRequestLogVendorAPI = 1002,
        ApiRequestLogGameApi = 1005,
        ApiRequestLogPublicApi = 1006,
        ApiRequestLogExternalVendorApi = 1007,
        ApiRequestLogJobAPI = 1029,


        // 錯誤Log
        // 錯誤Log
        // 錯誤Log
        ApiRequestLogAdminSysAPIError = 1501,
        ApiRequestLogVendorAPIError = 1502,
        ApiRequestLogGameAPIError = 1505,
        ApiRequestLogPublicAPIError = 1506,
        ApiRequestLogExternalVendorAPIError = 1507,
        ApiRequestLogJobAPIError = 1529,

        /// <summary>
        /// 三方/外部API請求紀錄
        /// </summary>
        ExternalApiRequestlog = 3001,

        /// <summary>
        /// 無縫錢包API請求紀錄
        /// </summary>
        SinglewalletMerchantApiRequestLog = 4001,

    }

}
