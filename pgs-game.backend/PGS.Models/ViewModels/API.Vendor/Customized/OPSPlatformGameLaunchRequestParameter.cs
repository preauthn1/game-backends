using System;

namespace target.Models.ViewModels
{
    public class OPSPlatformGameLaunchRequestParameter
    {
        public string token { get; set; }
        public string player_name { get; set; }
        public string lang { get; set; }

        /// <summary>
        /// 返回網址
        /// </summary>
        public string lobby { get; set; }
        public int game { get; set; }

        /// <summary>
        /// 是否為試玩模式
        /// </summary>
        public bool fun { get; set; }
    }

    public class OPSPlatformGameLaunchResponseModel : BaseOPSResponseModel
    {
        public string url { get; set; }
    }


    public class OPSPlatformScreenReplayRequestParameter
    {
        public int game { get; set; }
        public string round { get; set; }
        public string lang { get; set; }
    }


    public class OPSPlatformScreenReplayResponseModel: BaseQTechResponseModel
    {
        public string url { get; set; }
    }


    public class BaseOPSResponseModel 
    {
        public string code { get; set; }

        public string message { get; set; }

    }


}
