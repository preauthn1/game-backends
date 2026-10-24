using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper;

namespace game.Entity.Payment
{

    public class QueryPayGatewayRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public string PlatformIDsText { get; set; }

        public List<int> PlatformIDs { get; set; }

    }


}
