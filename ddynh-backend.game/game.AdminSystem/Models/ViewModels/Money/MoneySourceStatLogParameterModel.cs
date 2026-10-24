using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Money;

namespace game.AdminSystem
{
    public class MoneySourceStatLogParameterModel
    {
        public int? Platformids { get; set; }

        public int? AreaID { get; set; }

        public int? SourceTypeGroup { get; set; }

        public int? SourceType { get; set; }

        public MoneySourceChangeType? ChangeType { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsExport { get; set; }

    }
}