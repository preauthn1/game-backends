using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    public class AnnouncementViewModel
    {
        public List<AnnouncementItemViewModel> Items { get; set; }
    }


    public class AnnouncementItemViewModel
    {
        public int AnnouncementID { get; set; }

        public Guid AnnouncementGuid { get; set; }

        public int Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ImageUrlTop))
                    return ImageUrlTop;

                if (!string.IsNullOrWhiteSpace(ImageUrlMiddle))
                    return ImageUrlMiddle;

                return ImageUrlBottom;
            }
        }

        public string ImageUrlTop { get; set; }

        public string ImageUrlMiddle { get; set; }

        public string ImageUrlBottom { get; set; }

        public int LinkType { get; set; }

        public string LinkReference { get; set; }
    }

    public class AnnouncementReadRequestModel
    {
        public int AnnouncementID { get; set; }
    }


}
