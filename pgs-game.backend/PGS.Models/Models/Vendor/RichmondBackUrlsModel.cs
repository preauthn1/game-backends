using target.Models.ViewModels;
using System.Collections.Generic;

namespace target.Models.Models
{
    public class BackUrlsQueryParameterModel
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

    }



    public class VendorAPIBackUrlsViewModel
    {
        public List<string> Urls { get; set; }
    }
}
