using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Utility.ThirdParty
{
    public class SkypeMessageParameter
    {
        public SkypeMessageParameter()
        {

        }

        public string Message { get; set; }

        public string TargetConversationID { get; set; }

        public SkypeExtraSetting ExtraSetting { get; set; }

    }



    public class SkypeExtraSetting
    {    

        public string ImageUrl { get; set; }


        /// <summary>
        /// 不支援 PDF檔案 
        /// 不支援 Excel
        /// </summary>
        public List<SkypeAttachment> ListOfAttachment { get; set; }


        public SkypeAttachmentDataV2 AttachmentDataV2 { get; set; }


        /// <summary>
        /// 若成功發送request 會額外紀錄
        /// </summary>
        public Action<IMMessageSendCallbackParameter> LogActionCallback { get; set; }

    }



    public class SkypeAttachment
    {
        /// <summary>
        /// 不支援PDF
        /// </summary>
        public string contentType { get; set; }

        public string contentUrl { get; set; }

        public string name { get; set; }
    }


    public class SkypeAttachmentDataV2
    {
        public string contentType { get; set; }

        public string originalBase64 { get; set; }

        public string name { get; set; }
    }


}
