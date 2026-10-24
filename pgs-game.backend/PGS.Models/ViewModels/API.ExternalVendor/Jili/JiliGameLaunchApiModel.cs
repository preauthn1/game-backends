using System;

namespace target.Models.ViewModels
{
    public class JiliGameLaunchApiModel : JiliAPICommonResponse
    {       

        public string Data { get; set; }
    }


    public class JiliRecordDetailExternalUrlResultModel : JiliAPICommonResponse
    {
        public string Url { get; set; }

        public JiliRecordDetailExternalUrlDetailItemModel Data { get; set; }
    }


    public class JiliRecordDetailExternalUrlDetailItemModel
    {
        public string Url { get; set; }
    }

}
