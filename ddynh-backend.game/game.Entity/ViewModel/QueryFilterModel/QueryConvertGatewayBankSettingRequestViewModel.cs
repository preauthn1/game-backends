using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper;

namespace game.Entity.Game
{

    public class QueryConvertGatewayBankSettingRequestViewModel : PaginationParameter
    {
        public string GWIDsText { get; set; }

        public int? Status { get; set; }

        public List<int> IDs { get; set; }

        public List<int> GWIDs { get; set; }        
    }


}
