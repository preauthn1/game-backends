using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Payment
{
    ///[Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class CDMerchantList : BaseEntity
    {
        public string GWName { get; set; }
        public int GWID { get; set; }
        public string PlatformName { get; set; }
        public string PlatformId { get; set; }
        public int Info { get; set; }
        public int ID { get; set; }
        public string InfoData { get; set; }
        public string Uuid { get; set; }
        public int MerchantStatus { get; set; }
        public int Status { get; set; }
    }
}
