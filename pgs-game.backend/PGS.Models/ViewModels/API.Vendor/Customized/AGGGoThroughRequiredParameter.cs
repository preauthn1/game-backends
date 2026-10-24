using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGGoThroughRequiredParameter
    {
        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public List<int> GameIDs { get; set; }

        public string MerchantSecretKey { get; set; }

        public string Currency { get; set; }

        public string PlayerName { get; set; }
    }


    public class FreeSpinCampaignListResponse
    {
        public string requestId { get; set; }
        public List<FreeSpinCampaignItem> freeSpinCampaigns { get; set; }
        public int total { get; set; }
    }


    public class FreeSpinCampaignSetResponse
    {
        public string requestId { get; set; }
        public FreeSpinCampaignItem freeSpinCampaign { get; set; }
    }


    public class AGGFreeSpinStatusResponse
    {
        public string requestId { get; set; }
        public List<FreeSpinStatusItem> freeSpinStatuses { get; set; }
        public int total { get; set; }
    }


    public class AGGFreeSpinRecordResponse
    {
        public string requestId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string gameId { get; set; }
        public string freeSpinId { get; set; }
        public string userId { get; set; }
        public string wagerId { get; set; }
        public string clientId { get; set; }
        public int pageSize { get; set; }
        public int pageIndex { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Currency
    {
        public string currencyCode { get; set; }
        public bool isIso4217 { get; set; }
    }

    public class GameList
    {
        public string gameId { get; set; }
        public int bet { get; set; }
    }



    public class FreeSpinStatusItem
    {
        public string userId { get; set; }
        public string freeSpinId { get; set; }
        public int creditRate { get; set; }
        public string clientId { get; set; }
        public int lastRounds { get; set; }
        public int totalRounds { get; set; }
        public int totalWin { get; set; }
        public string freeSpinUseStatus { get; set; }
        public DateTime createDate { get; set; }
        public DateTime modifyDate { get; set; }
    }



    public class FreeSpinCampaignItem
    {
        public string freeSpinId { get; set; }
        public int creditRate { get; set; }
        public string clientId { get; set; }
        public int rounds { get; set; }
        public List<AGGFreeSpinGameList> gameList { get; set; }

        //public List<string> userIdList { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime useEndDate { get; set; }
        public string freeSpinStatus { get; set; }
        public bool enable { get; set; }
        public DateTime createDate { get; set; }
        public DateTime modifyDate { get; set; }
    }


    public class AGGFreeSpinGameList
    {
        public string gameId { get; set; }
        public int bet { get; set; }
    }



}
