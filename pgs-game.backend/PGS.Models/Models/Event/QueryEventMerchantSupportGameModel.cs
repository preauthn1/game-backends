using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryEventMerchantSupportGameModel
    {
        public int EventID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

    }


}
