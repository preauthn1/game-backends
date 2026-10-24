using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryEGamingDataMigratePendingQueueRequestViewModel : PaginationParameter
    {

        public bool IsLockQuery { get; set; }


        public List<string> BetRecordNos { get; set; }

        public bool? IsDone { get; set; }

    }


}
