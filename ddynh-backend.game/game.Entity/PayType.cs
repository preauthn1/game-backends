using FW.Core.Helper;

namespace game.Entity.Payment
{
    public class PayType : BaseEntity
    {
        public int GWID { get; set; }
        public int Type { get; set; }
        public string Currencies { get; set; }
        public string BankCode { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public bool AmountFixed { get; set; }
        public string FixedAmount { get; set; }
        public bool Status { get; set; }
        
        //public CDPayTypeStatus Status { get; set; }
        //public DateTime CreateTime { get; set; }
        //public DateTime? LastUpdateTime { get; set; }
        //public DateTime LastLogonTime { get; set; }
    }
}
