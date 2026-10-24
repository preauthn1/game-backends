using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class SlotManiaAPICommonResponse
    {

        public SlotManiaAPICommonResponse()
        {

        }


        public int code { get; set; }

        public string msg { get; set; }

    }

    public class SlotManiaAPIGenericResponseModel<T> : SlotManiaAPICommonResponse
    {
        public T Data { get; set; }
    }



    public class SMCreateLoginTokenResponseModel 
    {
        public string token { get; set; }

        public string username { get; set; }
    }

    public class SMScreenshotAPIResponseModel : SlotManiaAPICommonResponse
    {
        public string Data { get; set; }

    }

}
