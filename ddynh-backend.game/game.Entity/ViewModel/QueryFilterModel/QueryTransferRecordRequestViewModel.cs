using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryTransferRecordRequestViewModel : PaginationParameter
    {
        public List<int> UserIDs { get; set; }

        public List<int> GameMerchantIDs { get; set; }

        public List<int> PlatformIDs { get; set; }

        public string OrderNo { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }


    }


}
