using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UpdateUserBalanceParameterModel
    {

        public int UserID { get; set; }

        /// <summary>
        /// 对应 SourceTypeEnum
        /// </summary>
        public int SourceType { get; set; }
        public long CurrentAmount { get; set; }
        public long Amount { get; set; }
        public string IPAddress { get; set; }
        public string Memo { get; set; }
        public long TaxAmount { get; set; }
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
    }
}
