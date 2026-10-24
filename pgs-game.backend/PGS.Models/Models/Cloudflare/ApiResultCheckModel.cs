using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;

namespace target.Models.Models
{

    public class CloudflareIPRangeResult
    {
        public List<string> ipv4_cidrs { get; set; }
        public List<string> ipv6_cidrs { get; set; }
        public string etag { get; set; }
    }

    public class CloudflareIPRangeApiResponseModel
    {
        public CloudflareIPRangeResult result { get; set; }
        public bool success { get; set; }
    }


    public enum CloudflareDNSEnum
    {
        [Description("example.invalid")]
        DNS_888stars_publicAPI = 1001,
    }



    public class CloudflareDNSUpdateReqeust
    {
        public CloudflareDNSUpdateReqeust()
        {
            this.ttl = 120;
            this.type = "CNAME";
        }

        public string name { get; set; }
        public string content { get; set; }
        public bool proxied { get; set; }
        public string type { get; set; }
        public int ttl { get; set; }
    }

    public class CloudflareDNSZoneInformation
    {
        public string DNS_Name { get; set; }
        public string DNS_Content { get; set; }
        public string zone_id { get; set; }
        public string dns_id { get; set; }
    }



    public class CloudflareDNSUpdateResponse
    {
        public int code { get; set; }
        public string error { get; set; }
        public bool success { get; set; }
    }

}
