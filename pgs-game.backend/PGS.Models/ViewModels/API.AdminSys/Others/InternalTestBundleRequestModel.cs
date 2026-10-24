using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class InternalTestBundleRequestModel
    {
        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public string Lang { get; set; }

        public string PlatformSessionToken { get; set; }

        public bool DoNotIframe { get; set; }

        public string BackUrl { get; set; }
    }


    public class CustomerServiceUtilityResultModel
    {
        public string RefreshTimeTW { get; set; }

        public List<CustomerServiceUtilityWarningItem> WarningMessages { get; set; }

        public List<CustomerServiceSupportCurrencyInfoItem> SupportCurrency { get; set; }

        public List<CustomerServiceGroupInfo> GroupItems { get; set; }

        public string SpeedTestUrls_Basic { get; set; }

        public string SpeedTestUrls_Newest { get; set; }

    }


    public class CustomerServiceGroupInfo
    {
        public string GroupName { get; set; }

        public List<string> Items { get; set; }
    }



    public class CustomerServiceSupportCurrencyInfoItem
    {
        public string Currency { get; set; }

        public string CurrencyText { get; set; }

        public string Memo { get; set; }
    }



    public class CustomerServiceUtilityWarningItem
    {
        public bool IsErrorHint { get; set; }

        public string Message { get; set; }
    }



    public class InternalTestResultViewModel
    {

        public string Url { get; set; }

    }




}
