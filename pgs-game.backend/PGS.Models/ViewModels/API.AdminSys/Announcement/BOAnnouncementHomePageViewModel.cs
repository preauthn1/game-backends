using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class BOAnnouncementHomePageViewModel
    {

        /// <summary>
        /// 未讀的彈出型公告
        /// </summary>
        public List<HomePageBOAnnouncementItemViewModel> PopupAnns { get; set; }

        public List<HomePageBOAnnouncementItemViewModel> Items { get; set; }

    }


    /// <summary>
    /// 大部分欄位都直接繼承的
    /// </summary>
    public class HomePageBOAnnouncementItemViewModel : BOAnnouncementViewModel
    {
      
        public bool IsRead { get; set; }


        public string ContentStripHtml { get; set; }

    }




}
