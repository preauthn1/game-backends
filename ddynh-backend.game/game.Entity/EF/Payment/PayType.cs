using FW.Core.Helper;

namespace game.Entity.Payment
{
    public class PayType : BaseEntity
    {
        public string GWName { get; set; }
        public int GWID { get; set; }
        public int Type { get; set; }
        public string Currencies { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public bool AmountFixed { get; set; }
        public string FixedAmount { get; set; }
        public bool Status { get; set; }
        public string TradeCode { get; set; }
        public string DisplayName { get; set; }

        //public CDPayTypeStatus Status { get; set; }
        //public DateTime CreateTime { get; set; }
        //public DateTime? LastUpdateTime { get; set; }
        //public DateTime LastLogonTime { get; set; }
    }
}
