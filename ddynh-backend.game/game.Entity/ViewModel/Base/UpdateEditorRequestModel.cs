using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UpdateEditorRequestModel
    {
        public int TargetId { get; set; }

        /// <summary>
        /// 刪除的API 請帶這個參數
        /// </summary>
        public List<int> TargetIds { get; set; }

        public string UpdateBy { get; set; }

    }

}
