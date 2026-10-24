namespace target.Models.ViewModels
{
    public class PublicPageSupportGameItemViewModel
    {
        public int GameID { get; set; }

        public int GameType { get; set; }

        /// <summary>
        /// 這邊撈的是 i18n多語系
        /// </summary>
        public string GameName { get; set; }

        public string ImageUrl { get; set; }

        public string CheckCode { get; set; }

        public bool MustShowIframeBackButton { get; set; }

    }






}
