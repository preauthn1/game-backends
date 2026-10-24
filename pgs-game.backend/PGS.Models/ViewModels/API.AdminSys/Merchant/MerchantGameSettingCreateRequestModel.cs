using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class MerchantGameSettingUpdateRequestModel
    {
        public int GameID { get; set; }

        public int Status { get; set; }
    }



    public class MerchantGameSettingCopyRequestModel
    {
        /// <summary>
        /// 以誰的資料當基準複製
        /// </summary>
        public int SourceMerchantID{ get; set; }

        /// <summary>
        /// 將所有配置複製到此商戶
        /// </summary>
        public int CopyToMerchantID { get; set; }


        /// <summary>
        /// 是否刪除所有既有遊戲開關
        /// </summary>
        public bool IsDeleteOriginalGames { get; set; }

    }


    public class MultipleMerchantAdjustGameSettinRequestModel
    {

        /// <summary>
        /// 二選一操作
        /// </summary>
        public string RawText { get; set; }

        /// <summary>
        /// 二選一操作
        /// </summary>
        public int? ParentMerchantID { get; set; }

        /// <summary>
        /// 懶得建enum了 1:開啟  9:關閉
        /// </summary>
        public int Type { get; set; }


        public List<int> GameIDs { get; set; }      

    }


}
