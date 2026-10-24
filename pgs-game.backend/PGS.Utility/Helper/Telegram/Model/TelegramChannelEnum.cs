using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Utility.ThirdParty
{
    public enum TelegramChannelEnum
    {
        //查看TG cahtID 可以透過  https://example.invalid/bot1490132862:AAEV0BmAVOfWrXnnwNrfAvwT-nTqV_wbQ3Q/getUpdates

        [Description("-1001968758753")]
        pgs_merchant_statistic_report,

        /// <summary>
        /// 內部用PGS訊息預覽 (TG公告)
        /// </summary>
        [Description("-576451364")]
        internal_prview_content,


        /// <summary>
        /// 客服內部溝通群 (ION & target)
        /// </summary>
        [Description("-943125973")]
        internal_CS_cowork,

        [Description("-576451364")]
        pgs_common_error,

        [Description("-483617918")]
        pgs_single_wallet_erorr,

        [Description("-401633394")]
        pgs_job_api,

        [Description("-541624047")]
        pgs_externalAPI,

        [Description("-1002853440176")]
        pgs_adminSys_api,

        [Description("-505089736")]
        pgs_risk_control,

        [Description("-894381258")]
        pgs_rtp_warning,

        [Description("-943125973")]
        internal_channel_PGS_x_CustomerService,

        [Description("-922041387")]
        pgs_important_error,

    }

}
