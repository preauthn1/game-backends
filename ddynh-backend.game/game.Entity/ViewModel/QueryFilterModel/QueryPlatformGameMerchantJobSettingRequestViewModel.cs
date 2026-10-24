using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryPlatformGameMerchantJobSettingRequestViewModel : PaginationParameter
    {

        public List<int> JobTypes { get; set; }

        public List<int> JobCategories { get; set; }

        public List<int> PlatformIDs { get; set; }

        public List<int> GameMerchantIDs { get; set; }

        public bool IsEnableOnly { get; set; }

    }


}
