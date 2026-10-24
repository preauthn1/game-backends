using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{

    /// <summary>
    /// 繼承用 欄位名稱請勿亂改名 避免錯誤
    /// </summary>
    public abstract class BaseQueryRequestModel
    {
        public int? AreaID { get; set; }

        public int? PlatformID { get; set; }

        public int? AdminID { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

    }
}
