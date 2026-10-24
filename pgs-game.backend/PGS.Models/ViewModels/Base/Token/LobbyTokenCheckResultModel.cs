using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class LobbyTokenParsingResultModel
    {
        public PlayerTokenJwtModel ParsingTokenModel { get; set; }

        public GameLaunchRequestModel ParsingRawRequest { get; set; }

        public long QueryId { get; set; }


        //public string BackUrl { get; set; }

        //public string LiveCasinoLevelID { get; set; }

        //public int LobbyDefaultCategory { get; set; }
    }


}
