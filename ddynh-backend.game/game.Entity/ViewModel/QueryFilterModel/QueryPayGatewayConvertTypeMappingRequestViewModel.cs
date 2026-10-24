using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FW.Core.Helper;

namespace game.Entity.Game
{

    public class QueryPayGatewayConvertTypeMappingRequestViewModel : PaginationParameter
    {
        public string GWIDsText { get; set; }


        public List<int> GWIDs { get; set; }


        public List<int> IDs { get; set; }


    }


}
