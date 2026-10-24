using game.Entity.Game;
using Newtonsoft.Json;

namespace target.Models.ViewModels
{
    public class THubPlatformGetGameReplayScreenshotRequestParameter
    {
        public string PlayerCode { get; set; }

        public string GameCode { get; set; }

        public string GameRoundId { get; set; }

        public string Currency { get; set; }

        /// <summary>
        /// Start date. Format: yyyy-MM-ddTHH:mm:ss.fffzzz
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// End date. Format: yyyy-MM-ddTHH:mm:ss.fffzzz
        /// </summary>
        public string EndDate { get; set; }
    }


    public class THubPlatformGetGameReplayScreenshotResponseModel : BaseThubRequestResponseModel
    {
        [JsonProperty("Value")]
        public THubPlatformGetGameReplayScreenshotValueModel Value { get; set; }
    }


    public class THubPlatformGetGameReplayScreenshotValueModel
    {
        [JsonProperty("Url")]
        public string Url { get; set; }
    }
}
