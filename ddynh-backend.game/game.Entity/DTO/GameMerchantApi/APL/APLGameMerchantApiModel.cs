using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{


    public class APLGameMerchantApiQueryBetRecordRequestModel
    {

        public int PlatformID { get; set; }

        public DateTime BeginTimeUTC { get; set; }

        public DateTime EndTimeUTC { get; set; }

    }



    public class APLGameMerchantApiRequestModel
    {
        public int UserID { get; set; }

        public int PlatformID { get; set; }

    }

    public class APLGameMerchantApiQueryTransferRecordResultModel
    {
        public string type { get; set; }

        public decimal amt { get; set; }
    }


    public class APLGameMerchantApiGameLaunchResultModel
    {

        /// <summary>
        /// 站台
        /// </summary>
        public string site { get; set; }

        /// <summary>
        /// 登入成功后跳转的页面
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 登入成功请求参数
        /// </summary>
        public string query { get; set; }

    }


    public class APLGameMerchantApiWalletAmountResultModel
    {
        public decimal balance { get; set; }

    }

    public class APLGameMerchantApiWalletTransferResultModel
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string refno { get; set; }

        /// <summary>
        /// 编码后交易编号
        /// </summary>
        public string paymentId { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public string balance { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public string amt { get; set; }

    }



    public class APLGameMerchantApiCommonResultModel<T>
    {
        public string status { get; set; }

        public object error { get; set; }

        public T result { get; set; }

    }



}
