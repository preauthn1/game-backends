using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Utility
{
    public class RequestInfoTemporaryModel
    {

        public RequestInfoTemporaryModel()
        {
            this.ResponseCookies = new Dictionary<string, string>();
        }


        public RequestParameters RequestParameters { get; set; }
        
        public HttpWebRequest HttpWebRequest { get; set; }

        public string RequestContent { get; set; }

        public string ResponseContent { get; set; }

        public Dictionary<string,string> ResponseCookies { get; set; }

        public int ResponseStatusCode { get; set; }

        public long ResponseTime { get; set; }

        public System.Diagnostics.Stopwatch Timer { get; set; }

        


    }
}
