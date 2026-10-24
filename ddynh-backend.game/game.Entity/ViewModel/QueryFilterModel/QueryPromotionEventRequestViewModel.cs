using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryPromotionEventRequestViewModel : PaginationParameter
    {
        public int? Status { get; set; }

        public List<int> PlatformIDs { get; set; }


        public string Keyword { get; set; }

        public string PlatformIDsText { get; set; }

        /// <summary>
        /// 是否包含永久隐藏的资料
        /// </summary>
        public bool IsIncludePermanentInvisible { get; set; }

    }
}
