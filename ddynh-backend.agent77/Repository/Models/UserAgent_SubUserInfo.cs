using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_SubUserInfo
    {
        public int UserID { get; set; }
        public byte UserLevel { get; set; }
        public long SumTaxTotal { get; set; }
        public decimal SumTaxTotalCash
        {
            get
            {
                return Math.Round(((decimal)SumTaxTotal / 100), 2);
            }
        }
        public long SumTaxBonus { get; set; }
        public decimal SumTaxBonusCash
        {
            get
            {
                return Math.Round(((decimal)SumTaxBonus / 100), 2);
            }
        }
        public long SumTaxConvert { get; set; }
        public decimal SumTaxConvertCash
        {
            get
            {
                return Math.Round(((decimal)SumTaxConvert / 100), 2);
            }
        }
        public long SumTaxPerformed { get; set; }
        public decimal SumTaxPerformedCash
        {
            get
            {
                return Math.Round(((decimal)SumTaxPerformed / 100), 2);
            }
        }
        public long UnWithDrawTotal { get; set; }
        public decimal UnWithDrawTotalCash
        {
            get
            {
                return Math.Round(((decimal)UnWithDrawTotal / 100), 2);
            }
        }
        public System.DateTime CreateTime { get; set; }
        public Nullable<byte> Status { get; set; }
        public Nullable<int> TopUserID { get; set; }
        public Nullable<System.DateTime> LastUpdatetime { get; set; }
        public Nullable<int> PlatformID { get; set; }
        public Nullable<int> AreaID { get; set; }
        public string NickName { get; set; }
    }
}
