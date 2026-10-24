using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Pay
{
    /// <summary>
    /// GashPaySettle
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GashPaySettle: BaseEntity
    {
        public string MsgType { get; set; }
        public string PCode { get; set; }
        public string CId { get; set; }
        public string COId { get; set; }
        public string CUId { get; set; }
        public string PAId { get; set; }
        public string Amount { get; set; }
        public string ERQC { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
