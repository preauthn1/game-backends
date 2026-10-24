using Newtonsoft.Json;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantBriefViewModel : IMerchantBasic
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }

        public int Sequence { get; set; }
    }

}
