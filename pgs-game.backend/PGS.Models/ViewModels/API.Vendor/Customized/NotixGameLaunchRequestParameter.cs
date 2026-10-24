using game.Entity.Game;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class NotixGameLaunchRequestParameter
    {
        public string requestId { get; set; }
        public string gameId { get; set; }
        public string destinationId { get; set; }
        public string sessionId { get; set; }
        public NotixGameLaunchPlayerModel player { get; set; }
        public NotixPlayerBalanceModel balance { get; set; }
        public NotixGameBrowserSessionModel browserSession { get; set; }
        public object bet { get; set; }
        public NotixPlayerRedirectUrlModel redirectUrls { get; set; }
        public bool lobbyEnabled { get; set; }

    }


    public class NotixGameLaunchExtraParameter
    {
        public string ProviderID { get; set; }

        public bool IsTrialMode { get; set; }        
    }



    public class NotixPlayerRedirectUrlModel
    {
        public string returnUrl { get; set; }
        public string depositUrl { get; set; }
    }

    public class NotixGameBrowserSessionModel
    {
        public string ip { get; set; }
        public string language { get; set; }
        public string viewType { get; set; }
        public int timeZone { get; set; }

    }


    public class NotixPlayerBalanceModel
    {
        public string currency { get; set; }
        public string amount { get; set; }
    }


    public class NotixGameLaunchPlayerModel
    {
        public string id { get; set; }
        public string country { get; set; }
        public string nickname { get; set; }
    }



    public class NotixGameLaunchResultViewModel
    {
       
        public string url { get; set; }

    }


    public class NotixGameListResultViewModel
    {
        public List<NotixGameListItemViewModel> games { get; set; }
    }


    public class NotixGameListItemViewModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public bool isEnabled { get; set; }
        public bool isLive { get; set; }
        public string mobileHorizontalStatus { get; set; }
        public string imageUrl { get; set; }
        public List<string> currencies { get; set; }
        public List<string> languages { get; set; }
        public List<string> restrictedCountries { get; set; }
        public List<NotixPromoFeaturesModel> promoFeatures { get; set; }
        public DateTime releaseDate { get; set; }
    }



    public class NotixPromoFeaturesModel
    {
        public string type { get; set; }
        public string name { get; set; }
        public string status { get; set; }

    }


    public class NotixGameListRequestParameter
    {
        public string ProviderCode { get; set; }

    }


    public class NotixScreenshotAPIRequestParameter
    {
        public string ProviderCode { get; set; }

        public string GameId { get; set; }

        public string language { get; set; }

        public string playerId { get; set; }

        public string RoundId { get; set; }
    }


    public class NotixScreenshotAPIResultViewModel
    {
        public string reportUrl { get; set; }

    }


}
