using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Payment
{

    public class PayGatewayEntity
    {
        [Key]
        public int ID { get; set; }
        public string GatewayCode { get; set; }
        public string GatewayName { get; set; }
        public int MerchantInfo { get; set; }
        public string SettingInfo { get; set; }
        public bool SupportPayin { get; set; }
        public bool SupportPayout { get; set; }
        public bool ReviseAmount { get; set; }
        public string BankCodes { get; set; }
        public byte? Active { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string Url { get; set; }
        public byte Systems { get; set; }
        public int Sort { get; set; }
        public string UpdateBy { get; set; }

    }

}
