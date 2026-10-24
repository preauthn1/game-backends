using target.Models.Table;
using target.Models.ViewModels.GameVendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class PerGameLargeExportQueryModel
    {
        public string ThirdPartyBetRecordNo { get; set; }

        public decimal BetAmount { get; set; }

        public decimal UpdateAmount { get; set; }

        public DateTime ThirdSettleDate { get; set; }
    }



}
