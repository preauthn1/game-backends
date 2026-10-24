using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryJackpotAmountRequestParamete
    {
        public int VendorConfigID { get; set; }
    }

    public class AGGJackpotAmountViewModel
    {
        public List<AGGJackpotAmountItemViewModel> Items { get; set; }
    }

    public class AGGJackpotAmountItemViewModel
    {
        public int GroupID { get; set; }

        public string GroupName { get; set; }

        public string JPAmountName { get; set; }

        public decimal JPAmount { get; set; }

        public bool IsDefined { get; set; }

        public bool IsOddSequence { get; set; }
    }


}
