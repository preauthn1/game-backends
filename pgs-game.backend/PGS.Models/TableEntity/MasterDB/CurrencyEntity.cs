using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class CurrencyEntity
    {
        [Key]
        public string Currency { get; set; }

        public string Memo { get; set; }

        public decimal ExchangeRateToTWD { get; set; }

        public decimal ExchangeRateToUSDT { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public bool IsHot { get; set; }

        public bool IsDisableEdit { get; set; }

    }
}
