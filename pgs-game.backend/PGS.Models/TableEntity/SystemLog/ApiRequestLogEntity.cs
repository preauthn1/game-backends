using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.Table
{
    public class ApiRequestLogEntity
    {

        /// <summary>
        /// 新版都是儲存32碼單號了
        /// 另外請不要再寫入 sql server了
        /// </summary>
        [Key]
        public string UniqueID { get; set; }


        /// <summary>
        /// 有幾種途徑或取值 
        /// 嘗試解析 jwt GetUserIdentityTextFromJwt
        /// 從header取得 debug暫存
        /// </summary>
        public string UserIdentity { get; set; }
        public string RequestUrl { get; set; }
        public string Doamin { get; set; }
        public string RelativeUrl { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }

        /// <summary>
        /// 這邊紀錄的資訊會受到 HeaderLogLevelType 影響
        /// </summary>
        public string Headers { get; set; }
        public string ResponseText { get; set; }
        public int ResponseStatusCode { get; set; }
        public string IPAddress { get; set; }
        public string IPLocation { get; set; }
        public string Memo { get; set; }
        public string IdentityKey { get; set; }
        public DateTime CreateTime { get; set; }
        public int CreateTimeCategory { get; set; }
        public string StackTrace { get; set; }
        public string DebugInfo { get; set; }
        public long ResponseLength { get; set; }
        public string DebugInfo2 { get; set; }

        /// <summary>
        /// 成功reqeust 這一欄紀錄的時間是 API處理時間
        /// 失敗request 優先拿來記錄單號
        /// </summary>
        public string DebugInfo3 { get; set; }
        public string WebSiteName { get; set; }
        public int? ExecutionTime { get; set; }



        /// <summary>
        /// 此欄位不是table欄位
        /// </summary>
        [IgnoreWhenInsertUpdate]
        public int ExtraDebug_ErrorCode { get; set; }

    }


    public class ApiRequestLogExtraEntity: ApiRequestLogEntity
    {
        public string UniqueID { get; set; }

    }


}
