using target.Models.Table;
using System.Collections.Generic;

namespace target.Models.Models
{
    public class QueryMerchantInfoForVendorApiDocumentPageViewModel
    {

        public MerchantEntity QueryMerchant { get; set; }

        public List<DocumentGameListItemViewModel> Games { get; set; }

        public string IMChannelName { get; set; }

    }

    public class DocumentGameListItemViewModel
    {
        public int GameID { get; set; }
        public string GameNameEN { get; set; }
        public string GameNameCN { get; set; }
        public string GameTypeText { get; set; }
        public string ImageUrl { get; set; }
    }



}
