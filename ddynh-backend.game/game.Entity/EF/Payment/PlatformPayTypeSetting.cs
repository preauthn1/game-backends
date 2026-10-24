using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Payment
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PlatformPayTypeSetting : BaseEntity
    {
        public long PayTypeID { get; set; }

        public long PlatformID { get; set; }

        public int MinAmount { get; set; }

        public int MaxAmount { get; set; }

        public bool AmountFixed { get; set; }

        public string FixedAmount { get; set; }

        public int Status { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public int Priority { get; set; }


    }
}
