using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class SingleWalletMerchantSessionTokenExtraInfoEntity
    {
        [Key]
        public long Id { get; set; }

        public int PlayerID { get; set; }

        public string SessionTokenPGS { get; set; }

        public string SessionTokenRawLarge { get; set; }

        public string Info1 { get; set; }

        public string Info2 { get; set; }

        public string Info3 { get; set; }

        public string InfoJson { get; set; }
    }
}
