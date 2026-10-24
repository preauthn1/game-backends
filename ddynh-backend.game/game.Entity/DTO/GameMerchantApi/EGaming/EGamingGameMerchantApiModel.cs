using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{


    public class EGamingGameMerchantApiAccountExistCheckResultModel
    {
        public bool IsExist { get; set; }

        public string Msg { get; set; }

    }


    public class ThirdPartyGameTransferResultCheckModel
    {
        /// <summary>
        /// 轉帳API就直接拿到回應成功
        /// </summary>
        public bool IsFirstRequestResponseSuccess { get; set; }

        /// <summary>
        /// 二度詢問對方API回應成功
        /// </summary>
        public bool IsRecheckApiSuccess { get; set; }


        public bool IsUnknowError { get; set; }


        public string DebugMsg { get; set; }

    }



    public class EGamingGameMerchantApiModel
    {

        public string XXXXXXXXXX { get; set; }

    }

    public class EGamingGameMerchantApiQueryBetRecordFilterParameterModel
    {
        public int? UserID { get; set; }

        public int PlatformID { get; set; }

        public DateTime BeginTimeUTC { get; set; }

        public DateTime EndTimeUTC { get; set; }        


        /// <summary>
        /// 最多5000筆
        /// </summary>
        public int PageSize { get; set; }

        public int PageIndex { get; set; }

    }



    public class EGamingGameMerchantApiRequestModel
    {
        public int UserID { get; set; }

        public int PlatformID { get; set; }

    }

}
