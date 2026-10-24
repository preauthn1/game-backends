
using System;
using FW.Core.Repository;

namespace game.Entity.Pay
{
    [Database(Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class OrderLog
    {

        public int Id { get; set; }

        public int CSAdminId { get; set; }

        public string CSName { get; set; }

        public int OrderID { get; set; }

        public int UserID { get; set; }

        public float Amount { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

        public int PayTypeID { get; set; }

        public int GWID { get; set; }

        public string TypeName { get; set; }

        public string GatewayName { get; set; }
    }
}
