using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Utility.ThirdParty
{
    public class TGAPIChatObject
    {
        public string id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public bool all_members_are_administrators { get; set; }
    }


    public class TGAPIMessageObject
    {
        public int message_id { get; set; }
        public TGAPIChatObject chat { get; set; }
        public int date { get; set; }
    }

    
    public class TGAPIResultDetail
    {
        public int update_id { get; set; }
        public TGAPIMessageObject message { get; set; }
    }

    public class TGAPIGetUpdateResponse
    {
        public bool ok { get; set; }
        public List<TGAPIResultDetail> result { get; set; }
    }

    


}
