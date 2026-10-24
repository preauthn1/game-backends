using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryUserGameInfoRequestModel : PaginationParameter
    {        

        public List<int> UserIDs { get; set; }

        public string UserIDsText { get; set; }

        public List<int> GameIDs { get; set; }

        public string GameIDsText { get; set; }

        public List<int> PlatformIDs { get; set; }

        public string PlatformIDsText { get; set; }

        public string LoginName { get; set; }

    }
}
