using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.AzureFunctionV2
{
    public static class EazyRequestHelper
    {        
        public static void SendHttpRequest(string targetUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(targetUrl).Result;
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch
            {

            }
        }

    }
}
