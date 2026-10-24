using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using target.Models.ViewModels.API.AdminSys;
using target.Models.Enums;

namespace target.Models.Models
{

    public class TgRenameChannelTitleParameter
    {
        public string Chat_ID { get; set; }

        public string NewTitleName { get; set; }
    }

    public class TgNewChannelJoinParameter
    {

        public string Chat_ID { get; set; }

        public string ChannelName { get; set; }

        public bool IsFromInternalUser { get; set; }


    }


}
