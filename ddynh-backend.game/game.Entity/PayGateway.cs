using System;
using FW.Core.Helper;

namespace game.Entity.Payment
{
    public class PayGateway : BaseEntity
    {
        //GatewayCode, GatewayName, MerchantInfo, SupportPayin, SupportPayout, ReviseAmount, BankCodes, CreateTime
        public string GatewayCode { get; set; }
        public string GatewayName { get; set; }
        public int MerchantInfo { get; set; }
        public bool SupportPayin { get; set; }
        public bool SupportPayout { get; set; }
        public bool ReviseAmount { get; set; }
        public string BankCodes { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        //public DateTime? LastUpdateTime { get; set; }
    }
}
