using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class AnnouncementPagingViewModel
    {
        public int AnnouncementID { get; set; }

        public Guid AnnouncementGuid { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool IsExpired { get; set; }

        public int? MerchantSelectTemplateID { get; set; }
        public bool IsInProgress { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }
    }


    public class AnnouncementDetailViewModel
    {
        public int AnnouncementID { get; set; }

        public Guid AnnouncementGuid { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public string UpdateBy { get; set; }

        /// <summary>
        /// 為了配合前端方便 轉成此格式
        /// </summary>
        public long BeginTimeTS_1000 { get; set; }

        public long EndTimeTS_1000 { get; set; }


        public List<int> AssignMerchants { get; set; }


        public int? MerchantSelectTemplateID { get; set; }


        public List<AnnouncementContentBlockItemModel> Contents { get; set; }


    }



    public class AnnouncementContentBlockItemModel
    {
        public int ContentBlockID { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageText { get; set; }
        public Guid? OwnerGuid { get; set; }
        public string Memo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrlTop { get; set; }
        public string ImageUrlMiddle { get; set; }
        public string ImageUrlBottom { get; set; }
        public int LinkType { get; set; }
        public string LinkReference { get; set; }
        public bool IsDefault { get; set; }


        //以下為控制前端畫面的開關
        public bool IsDisableEditTitle { get; set; }

        public bool IsDisableEditDescriptiion { get; set; }
    }



    public class AnnouncementCreateRequestModel
    { 

        public string Memo { get; set; }

        public int? Type { get; set; }


    }


}
