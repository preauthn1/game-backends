using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventPrizeItemInitialModel
    {

        public EventPrizeItemInitialModel()
        {

        }
        public EventPrizeItemInitialModel(EventPrizeTypeEnum type)
        {
            this.PrizeType = (int)type;
        }

        public int PrizeType { get; set; }
        public decimal AmountOrQuantity { get; set; }
        public decimal? ProbabilityWeights { get; set; }
        public int? LimitQuantity { get; set; }

    }



    public class EventMultplePrizeBoxItemModel
    {
        /// <summary>
        /// 獎項次序 (從1開始)
        /// </summary>
        public int Sequence { get; set; }

        public int EventDrawPrizeItemID { get; set; }

        public decimal AmountOrQuantity { get; set; }

    }


}
