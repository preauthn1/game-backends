using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace target.Models.ViewModels
{



    public class DemoGameCategoryViewModels
    {      
        public int GameType { get; set; }

        public string GameTypeName { get; set; }

        public List<DemoGameItemViewModels> Games { get; set; }

    }
    

    public class DemoGameItemViewModels
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public string GameCode { get; set; }

        public string ImageUrl { get; set; }

        public int GameType { get; set; }

        public List<DemoServerItemViewModels> Servers { get; set; }


        public bool IsShowExtraInfo { get; set; }

        public string MaximumWin { get; set; }

        public string RTP { get; set; }

        public string LineWays { get; set; }


    }


    public class DemoServerItemViewModels
    {
        public int ServerID { get; set; }

        public string ServerName { get; set; }

        public int GameID { get; set; }

        [JsonIgnore]
        public int GameType { get; set; }

        public int FlagLevel { get; set; }
    }


}
