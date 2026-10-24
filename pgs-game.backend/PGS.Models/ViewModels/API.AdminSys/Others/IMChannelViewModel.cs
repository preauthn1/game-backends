using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class IMChannelViewModel
    {
        public int IMChannelID { get; set; }

        public string Type { get; set; }

        public string InternalMemo { get; set; }

        public string ThirdPartyChatID { get; set; }

        public string Memo2 { get; set; }

        public string Memo3 { get; set; }

        public string ChannelName { get; set; }

        public int MerchantCount { get; set; }

        public bool IsMappingProductionMerchant { get; set; }

        public int ReferIPTemplateID { get; set; }

        public string PreferLangText { get; set; }

        public string ReferIPTemplateName { get; set; }

        public List<string> Tags { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateBy { get; set; }

    }

    public class IMChannelCreateRequestModel
    {
        /// <summary>
        /// 參考 IMChannelTypeEnum
        /// </summary>
        public int Type { get; set; }


        public string ChannelName { get; set; }


        /// <summary>
        /// 只有透過TG_hook的狀況下 才會有值
        /// </summary>
        public string TG_Chat_ID { get; set; }

    }

    public class IMChannelUpdateRequestModel
    {

        public int IMChannelID { get; set; }


        public string ChannelName { get; set; }


        public string Memo { get; set; }


        public string TargetChatID { get; set; }



        public int? ReferIPAddressTemplateID { get; set; }


    }


    public class IMChannelRequestParameterModel : QueryIMChannelRequestViewModel
    {

        public int? AssignMerchantID { get; set; }


        /// <summary>
        /// 對應 SpecialFilterTypeOfIMChannels
        /// </summary>
        public int? SpecialFilterType { get; set; }



    }


}
