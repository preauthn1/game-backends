using System;
using FW.Core.Helper;

namespace game.Entity.Payment
{
    public class PayGateway : BaseEntity
    {
        //GatewayCode, GatewayName, MerchantInfo, SupportPayin, SupportPayout, ReviseAmount, BankCodes, CreateTime
        public int GWID { get; set; }
        public string GatewayCode { get; set; }
        public string GatewayName { get; set; }
        public int MerchantInfo { get; set; }
        public int SettingInfo { get; set; }
        public bool SupportPayin { get; set; }
        public bool SupportPayout { get; set; }
        public bool ReviseAmount { get; set; }
        public string BankCodes { get; set; }
        public byte Status { get; set; }
        public int Systems { get; set; }
        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = (String.IsNullOrEmpty(value)) ? "" : value;
            }
        }
        public DateTime CreateTime { get; set; }
    }
}
