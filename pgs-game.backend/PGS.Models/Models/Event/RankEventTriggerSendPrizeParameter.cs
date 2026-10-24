using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class RankEventTriggerSendPrizeParameter
    {
        /// <summary>
        /// 排名依據欄位 (可填寫NULL)
        /// </summary>
        public EventCounterOrderByEnum? OrderBy { get; set; }

    }


}
