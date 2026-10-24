using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{

    /// <summary>
    /// 這一家 https://example.invalid/
    /// </summary>
    public class ExchangerateApiResponseModel
    {
        public string result { get; set; }
        public string documentation { get; set; }
        public string terms_of_use { get; set; }
        public string time_last_update_utc { get; set; }
        public string time_next_update_utc { get; set; }
        public string base_code { get; set; }
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }



}
