using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class FumaAPICommonResponse
    {

        public FumaAPICommonResponse()
        {
            this.status = "0000";
        }


        public string status { get; set; }

        public string err_text { get; set; }

    }



    public class FumaAPICommonRequest
    {
        public FumaAPICommonRequest()
        {

        }

        /// <summary>
        /// Fuma提供的代號 (域名編號?)
        /// </summary>
        public string dc { get; set; }

        /// <summary>
        /// Fuma加密的body
        /// </summary>
        public string x { get; set; }
    }



}
