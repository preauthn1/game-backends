using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class PromotionEventUpdateModel
    {
        public int EventId { get; set; }

        public string Title { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }

        public int PlatformID { get; set; }

        public DateTime? EventStartTime { get; set; }

        public DateTime? EventEndTime { get; set; }
               
        public string UpdateBy { get; set; }


        public bool IsUpdatePage { get; set; }

    }


    public class PromotionEventItemUpdateModel : PromotionEventItemEntity
    {
   
        public string EventName { get; set; }

    }






}