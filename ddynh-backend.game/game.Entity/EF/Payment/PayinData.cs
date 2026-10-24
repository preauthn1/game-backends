using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Payment
{
    public class PayinData
    {
        public string thirdPartyCode { get; set; }
        public string gatewayMethod { get; set; }
        public string gatewayUrl { get; set; }
        private string _gatewayCode;
        public string gatewayCode
        {
            get
            {
                return _gatewayCode;
            }
            set
            {
                _gatewayCode = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
        public Dictionary<string, string> gatewayParam { get; set; }
        public string gatewayType { get; set; }
        public string orderNo { get; set; }
        public double? revisedAmount { get; set; }
        public string errorCode { get; set; }
        public string msg { get; set; }
    }

    public class AEPaymentResult
    {
        public string thirdPartyCode { get; set; }
        public string thirdPartyName { get; set; }
        public string currencies { get; set; }
        public object payinSetting { get; set; }
        public object payoutSetting { get; set; }
        public int requiredMerchantInfo { get; set; }
        public bool supportPayin { get; set; }
        public bool supportPayout { get; set; }
        public bool reviseAmountByResponse { get; set; }
        public object bankCodes { get; set; }
    }
}
