using Newtonsoft.Json;
using target.Models.Enums;
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

    public class MerchantCustomizedSettingQueryParameter
    {
        public int? SpecificKey { get; set; }

        public List<int> SettingKeys { get; set; }

    }


}
