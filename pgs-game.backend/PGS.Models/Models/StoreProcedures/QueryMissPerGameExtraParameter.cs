using target.Models.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class QueryMissPerGameExtraParameter
    {

        public List<int> ExcludePlayerIDs { get; set; }


        /// <summary>
        /// 如果有標記帳務排除的單 不會檢查警示
        /// </summary>
        public bool IsExcludeFinanceIgnore { get; set; }

    }


}
