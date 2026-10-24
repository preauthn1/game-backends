using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PublicPageEventDescriptionRequestViewModel
    {
        public int EventID { get; set; }

        public string CheckCode { get; set; }

        public bool IsPreview { get; set; }

        public string TargetLanguage { get; set; }
    }


    public class PublicPageEventDescriptionViewModel
    {
        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsLoginStatus { get; set; }

        public bool IsHideBeginEndTime { get; set; }

        public int ColorType { get; set; }

        public int BrandingType { get; set; }

        public string DisplayBeginTimeTW { get; set; }

        public string DisplayEndTimeTW { get; set; }

        public int RemaningSeconds { get; set; }

        public List<PublicPageSupportGameItemViewModel> SupportGames { get; set; }

        public EventDescriptionContentBlockViewModel TopTitle { get; set; }

        public List<EventDescriptionContentBlockViewModel> Contents { get; set; }
    }


    public class EventDescriptionContentBlockViewModel
    {
        public string LanguageCode { get; set; }
        public int LayoutType { get; set; }
        public Guid? OwnerGuid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrlTop { get; set; }
        public string ImageUrlMiddle { get; set; }
        public string ImageUrlBottom { get; set; }
        public bool IsForPreviewOnly { get; set; }
    }


}
