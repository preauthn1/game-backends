using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
    
    public class PromotionEventListPageViewModel
    {

        public int EventId { get; set; }

        public string Title { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public int PlatformID { get; set; }

        public string PlatformName { get; set; }

        public string IsHomePageVisible { get; set; }

        public string IsAnnounementPageVisible { get; set; }

        public string EventStartTime { get; set; }

        public string EventEndTime { get; set; }

        public string CreateTime { get; set; }

        public string UpdateTime { get; set; }

        public string UpdateBy { get; set; }

        public int TotalAreaCount { get; set; }

    }
}