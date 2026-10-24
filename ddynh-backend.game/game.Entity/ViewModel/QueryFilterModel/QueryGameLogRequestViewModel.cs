using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryGameLogRequestViewModel : PaginationParameter
    {
        public QueryGameLogRequestViewModel()
        {
            IsNoLockQuery = true;
        }


        public List<int> UserIDs { get; set; }

        public List<int> PlatformIDs { get; set; }

        public List<int> AreaIDs { get; set; }


        public DateTime? LoginTimeBeginTime { get; set; }

        public DateTime? LoginTimeEndTime { get; set; }

        public bool IsNoLockQuery { get; set; }

    }


}
