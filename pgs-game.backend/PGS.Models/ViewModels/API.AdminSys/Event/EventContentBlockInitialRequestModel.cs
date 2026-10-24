using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class EventContentBlockInitialRequestModel
    {
        public int EventID { get; set; }
    }


    public class EventContentBlockEditViewModel
    {
        public int EventID { get; set; }

        public string EventName { get; set; }

        public string ProductionUrl { get; set; }

        public string PreviewUrl { get; set; }

        public bool IsNotInitial { get; set; }

        /// <summary>
        /// 參考 EventContentColorStypeType
        /// </summary>
        public int ColorStyleType { get; set; }

        public bool IsUseOtherEventContent { get; set; }

        public int? ReferenceContentEventID { get; set; }

        public List<EventContentBlockParentViewModel> EventContentBlocks { get; set; }
    }


    public class EventContentBlockParentViewModel
    {
        public int EventContentBlockID { get; set; }

        public int EventID { get; set; }

        public Guid EventContentBlockGuid { get; set; }

        public string Memo { get; set; }

        public int LayoutType { get; set; }

        public string LayoutTypeText { get; set; }

        public int StyleType { get; set; }

        public int Sequence { get; set; }

        public int Status { get; set; }

        public List<AdminSysContentBlockItemViewModel> Contents { get; set; }
    }

    public class AdminSysContentBlockItemViewModel
    {
        public int ContentBlockID { get; set; }

        public string LanguageCode { get; set; }

        public string LanguageText { get; set; }

        public Guid OwnerGuid { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrlTop { get; set; }
        public string ImageUrlMiddle { get; set; }
        public string ImageUrlBottom { get; set; }
    }



}
