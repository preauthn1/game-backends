using System;

namespace target.Models.ViewModels
{
    public class QTechPlatformGameLaunchRequestParameter
    {
        public string operatorId { get; set; }
        public string playerId { get; set; }
        public int gameId { get; set; }
        public string displayName { get; set; }
        public string currency { get; set; }
        public string language { get; set; }
        public string mode { get; set; }
        public string device { get; set; }
        public string returnUrl { get; set; }
        public string sessionToken { get; set; }
    }

    public class QTechPlatformGameLaunchResponseModel : BaseQTechResponseModel
    {
        public string url { get; set; }
    }


    public class QTechPlatformScreenReplayRequestParameter
    {
        public string operatorId { get; set; }
        public string playerId { get; set; }
        public int gameId { get; set; }
        public string roundId { get; set; }
    }


    public class QTechPlatformScreenReplayResponseModel: BaseQTechResponseModel
    {
        public string url { get; set; }
    }


    public class BaseQTechResponseModel 
    {
        public string code { get; set; }

        public string message { get; set; }

    }

    public class QTechTestAPICreateSessionTokenModel : BaseQTechResponseModel
    {
        public string sessionToken { get; set; }

    }

    public class QTechTestAPIAuthTokenModel : BaseQTechResponseModel
    {
        public string access_token { get; set; }

    }

    public class QTechUrlResultModel : BaseQTechResponseModel
    {
        public string Url { get; set; }

    }


}
