using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class ApiRequestLog_ExternalApiRawEntity
    {
        [Key]
        public long Id { get; set; }
        public string Category { get; set; }
        public string RequestUrl { get; set; }
        public string RequestContent { get; set; }
        public string RequestBody { get; set; }
        public string RequestMethod { get; set; }
        public string ResponseText { get; set; }
        public long ResponseTime { get; set; }
        public int ResponseStatusCode { get; set; }
        public string WebSiteName { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
