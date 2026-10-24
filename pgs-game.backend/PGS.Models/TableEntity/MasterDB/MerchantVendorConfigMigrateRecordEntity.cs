using System;
using System.ComponentModel.DataAnnotations;

namespace target.Models.Table
{
    public class MerchantVendorConfigMigrateRecordEntity
    {
        [Key]
        public int Id { get; set; }
        public int MerchantID { get; set; }
        public int GameVendor { get; set; }
        public DateTime StatTimeBegin { get; set; }
        public DateTime StatTimeEnd { get; set; }
        public int VendorConfigIDBefore { get; set; }
        public int VendorConfigIDAfter { get; set; }
    }
}
