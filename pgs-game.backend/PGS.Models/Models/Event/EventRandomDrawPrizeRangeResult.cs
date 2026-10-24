using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventRandomPrizeDrawBox
    {
        /// <summary>
        /// 不含該值本身 MaxValue減一 才是最大有效值
        /// </summary>
        public long MaxValue { get; set; }

        public List<EventRandomDrawPrizeRangeItemModel> ItemsRange { get; set; }
    }

    public class EventRandomDrawPrizeRangeItemModel
    {
        public long Begin { get; set; }

        /// <summary>
        /// 不含此數字 (e.g. End=10  則代表0~9有效。不含10)
        /// </summary>
        public long End { get; set; }

        public int PrizeItemID { get; set; }
    }


}
