using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class PublicApiBetRecordReplayInfoViewModel
    {
        public BetRecordBasicInfo BasicInfo { get; set; }

        public PerGameSlotHistoryDetailInfoViewModel PgsSlotsHistoryInfo { get; set; }

        public bool IsFakeDate { get; set; }

        /// <summary>
        /// 某些細節資訊 只好顯示少數幾個欄位 其他隱藏
        /// </summary>
        public bool IsShowBriefAmountOnly { get; set; }

        public string RawHtml { get; set; }

        public List<string> ListOfRoundResultText { get; set; }

        public List<AGGLiveBackOfficeRecordDetailApiBetList> BetList { get; set; }

        public List<RoundInfoItemModel> SettleResultImageList { get; set; }
    }


}
