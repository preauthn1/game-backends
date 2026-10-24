using target.Models.Enums;
using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
    public class AGGGetGameRTPSettingReqeustParameter
    {
        public int VendorConfigID { get; set; }

        public int GameID { get; set; }

        public string DebugReferenceUUID { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }

    }


    public class AGGGetPlayerRTPSettingReqeustParameter
    {
        public int PlayerID { get; set; }

        public int GameID { get; set; }


        public string DebugReferenceUUID { get; set; }


        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }


    }



    public class AGGUpdateRTPSettingReqeustParameter
    {
        public int VendorConfigID { get; set; }

        public int GameID { get; set; }

        public int RTP { get; set; }

        public decimal? WinLimit { get; set; }

        public string DebugReferenceUUID { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }

    }

    public class AGGUpdatePlayerRTPSettingV2ReqeustParameter
    {
        public int PlayerID { get; set; }

        public List<int> GameIDs { get; set; }

        public int RTP { get; set; }

        public decimal? WinLimit { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }
    }



    public class AGGGoThroughReqeustParameter
    {
        public AGGGoThroughReqeustParameter()
        {
            this.CommonParams = new AGGGoThroughRequiredParameter();
        }

        public AGGGoThroughRequiredParameter CommonParams { get; set; }

        public Dictionary<string, object> KeyValues { get; set; }

    }







    public class AGGGetPlayerRTPSettingV2ReqeustParameter
    {
        public int PlayerID { get; set; }

        public int GameID { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }
    }


    public class AGGGetPlayerRTPSettingV2ResponseModel
    {
        public decimal Rtp { get; set; }

        public decimal WinLimit { get; set; }

        public decimal WinMultiLimit { get; set; }
    }

    public class AGGAPICommonResponseModel
    {
        public string RequestId { get; set; }

    }

    public class AGGUpdatePlayerRTPSettingReqeustParameter
    {
        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public int RTP { get; set; }

        public string DebugReferenceUUID { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }
    }


    public class AGGGetRTPSettingResponseModel
    {

        public int rtp { get; set; }

        public string modeHigh { get; set; }

        public string modeLow { get; set; }

        public int threshold { get; set; }

        public decimal winLimit { get; set; }

    }


    public class AGGGetAvailableRTPListResponseModel
    {



    }






}
