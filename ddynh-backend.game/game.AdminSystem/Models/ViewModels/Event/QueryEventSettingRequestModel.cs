using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class QueryEventSettingRequestModel 
    {
        public int? PlatformID { get;set;}
              
        public int? Status { get; set; }         

        public string Keyword { get; set; }

    }
}