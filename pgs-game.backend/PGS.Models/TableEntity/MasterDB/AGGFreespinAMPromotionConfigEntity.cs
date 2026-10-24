using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AGGFreespinAMPromotionConfigEntity
    {
        [Key]
        public int Id { get; set; }

        public int MerchantID { get; set; }

        /// <summary>
        /// 暫無對應enum  第一波活動 = 1
        /// </summary>
        public int Type { get; set; }

        public string Name { get; set; }

        public decimal DefaultBetAmount { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime EventEndTime { get; set; }


        public string ReferBudgetID { get; set; }
    }
}
