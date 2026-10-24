using System;

namespace target.Models.ViewModels
{
    public class LobbyGamesViewModel
    {
        public int GameVendorID { get; set; }

        public int GameID { get; set; }

        public int GameType { get; set; }

        public string GameName { get; set; }

        public string CheckCode { get; set; }

        public string GameImageUrl { get; set; }

        public bool IsMaintain { get; set; }

        public int Sequence { get; set; }

        /// <summary>
        /// 近期玩過
        /// </summary>
        public bool IsRecentlyGame { get; set; }

        public bool IsHotGame { get; set; }

        public bool IsNewGame { get; set; }

        /// <summary>
        /// 若有啟用iframe，必須強制導到iframe頁面 並採用顯示返回按鈕的版型
        /// </summary>
        public bool MustShowIframeBackButton { get; set; }

        public DateTime LastRecentlyTime { get; set; }

        public DateTime GameCreateTime { get; set; }

    }


}
