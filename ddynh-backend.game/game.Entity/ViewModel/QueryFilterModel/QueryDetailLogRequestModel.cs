using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryDetailLogRequestModel : PaginationParameter
    {        
        public int? UserID { get; set; }
        public int? GameID { get; set; }
        public int? ServerID { get; set; }
        public long? DealNo { get; set; }
        public string BeginTime { get; set; }
        public string EndTime { get; set; }

        /// <summary>
        /// 牌局號
        /// </summary>
        public string DeskCode { get; set; }
        public int? AdminID { get; set; }
    }
}
