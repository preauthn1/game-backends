using Newtonsoft.Json;
using target.Models.ViewModels.GameVendor;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGAPIResponseModel
    {
        public AGGAPIResponseModel()
        {
            
        }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }



        [JsonProperty(PropertyName ="message" , NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }


    }


    public class AGGGameBettingTemplateAPIResponseModel
    {
        public string requestId { get; set; }
        public List<AGGIntegratorBetDetail> integratorBets { get; set; }
        public int total { get; set; }
    }


    public class GameBetRangeList
    {
        public string gameId { get; set; }
        public string gameName { get; set; }
        public List<decimal> betRange { get; set; }
    }




    public class AGGIntegratorBetDetail
    {
        public Currency currency { get; set; }
        public decimal creditRate { get; set; }
        public decimal actualBetMin { get; set; }
        public decimal actualBetMax { get; set; }
        public string betTemplateApplyId { get; set; }
        public string betTemplateId { get; set; }
        public List<GameBetRangeList> gameBetRangeList { get; set; }
    }




    public class AGGBonusRetryRecordsResponseModel
    {
        public List<AGGBonusRetryRecordsDetailItemModel> Records { get; set; }

    }


    public class AGGBonusRetryRecordsDetailItemModel
    {
        public int UserId { get; set; }

        public string TransactionId { get; set; }
    }


}
