using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class GameListEntity : IGamei18nName
    {
        [Key]
        public int GameID { get; set; }

        public string GameName { get; set; }

        public string GameCode { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public string ThirdPartySecondCode { get; set; }


        public string LanguageCode { get; set; }


        /// <summary>
        /// 對應 GameTypeEnum
        /// </summary>
        public int GameType { get; set; }

        /// <summary>
        /// 對應 GameVendorTypeEnum
        /// </summary>
        public int GameVendorType { get; set; }

        public int Status { get; set; }

        public bool IsSkipInsertZeroBet { get; set; }

        public bool IsHideAfterAmountInfo { get; set; }

        public bool IsSupportSingleWallet { get; set; }

        public bool IsSupportTrialMerchantLaunch { get; set; }

        public bool IsSupportForAccountingIgnorePlayer { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsPrePaidMode { get; set; }

        /// <summary>
        /// 有時候會暫存 遊戲英文名稱 (批次寫入的時候)
        /// </summary>
        public string Memo { get; set; }
    }


    public class GameListIdInfoBrief
    {
        public int GameID { get; set; }

        public string GameCode { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public int Status { get; set; }
    }



    public interface IGamei18nName
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public string LanguageCode { get; set; }

        public string GameCode { get; set; }
    }


}
