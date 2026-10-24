using Newtonsoft.Json;
using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    /// <summary>
    /// 請求參數
    /// </summary>
    public class PlayerLogoutRequestModel
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        [SwaggerApiRequiredField]
        public string PlayerName { get; set; }
    }


    public class PlayerLogoutResultViewModel
    {
        public string PlayerName { get; set; }
    }

}
