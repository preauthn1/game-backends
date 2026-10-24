using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryGameMerchantPlatformConfigRequestViewModel : PaginationParameter
    {

        public List<int> GameMerchantIDs { get; set; }

        public List<int> PlatformIDs { get; set; }

    }


}
