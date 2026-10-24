using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Pay
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class RedirectData : BaseEntity
    {
        public int GWID
        {
            get;
            set;
        }


        public string FormData
        {
            get;
            set;
        }

        public DateTime CreateTime
        {
            get;
            set;
        }

        public DateTime ExpireTime
        {
            get;
            set;
        }

        public string GUID
        {
            get;
            set;
        }
    }
}
