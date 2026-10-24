using target.Models.Enums;

namespace target.Models.Models
{
    public class LobbyUrlGenerateModel
    {
        public string Url { get; set; }

        public string UrlWithoutFolder { get; set; }

        public LobbyMainApiVersionEnum ApiDomainVersion { get; set; }
    }



}
