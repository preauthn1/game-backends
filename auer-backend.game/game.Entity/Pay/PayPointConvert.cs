using FW.Core.Helper;
using FW.Core.Repository;
using System;

namespace game.Entity.Pay
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PayPointConvert : BaseEntity
    {
        public Int64 Amount { get; set; }
        public Int64 PointAmount { get; set; }
        public Int64 Bonus { get; set; }
        public int PayTypeID { get; set; }
    }
}
