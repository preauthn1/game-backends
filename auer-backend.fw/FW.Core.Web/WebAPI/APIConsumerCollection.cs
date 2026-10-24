using System;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Web.WebAPI
{
    public class APIConsumerCollection : List<APIConsumerInfo>
    {
        public bool CheckApi(string cid, string key)
        {
            return this.Where(a => a.CID == cid && a.Key == key).FirstOrDefault() != null;
        }
    }
}
