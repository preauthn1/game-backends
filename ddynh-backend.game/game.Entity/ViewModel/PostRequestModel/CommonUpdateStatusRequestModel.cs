using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{

    /// <summary>
    /// 泛用型的requestModel (修改狀態的常用欄位)
    /// </summary>
    public class CommonUpdateStatusRequestModel
    {        
        public int TargetId { get; set; }

        public int TargetStatus { get; set; }
        
        public string UpdateBy { get; set; }

        public string Memo { get; set; }

    }
}
