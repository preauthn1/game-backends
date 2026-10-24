using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class MerchantEventSummaryDataEntity
    {
        [Key]
        public int MerchantID { get; set; }

        [Key]
        public DateTime StatTime { get; set; }

        [Key]
        public int EventID { get; set; }


        [Key]
        public int EventPrizeType { get; set; }



        public int TotalPlayers { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime UpdateTime { get; set; }


    }


    public class MerchantEventSummaryExtraGroupModel : MerchantEventSummaryDataEntity
    {
       

    }
  







}
