using target.Models.Enums;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
    public class ExternalVendorGetSingleBetRecordRequestParameter
    {       
        public int MerchantID { get; set; }


        /// <summary>
        /// 三方遊戲代碼
        /// </summary>
        public string GameCode { get; set; }

        /// <summary>
        /// 要帶三方的單號
        /// </summary>
        public string BetRecordNo { get; set; }


        public QueryReplayExtraParameter QueryReplayExtraParameter { get; set; }
    }



    public class QueryReplayExtraParameter
    {
        public string DebugReferenceUUID { get; set; }

        public string AGGLiveVCode { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }
    }


    public class IDGLobbyEventChangeEnableFlagParameter
    {
        public int MerchantID { get; set; }


        public bool IsEnable { get; set; }

    }




}
