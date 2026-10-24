using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;

namespace game.Entity.Payment
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PayTypeCategoryMapping : BaseEntity
    {
        public int Id { get; set; }

        public int SourcePayType { get; set; }

        public int RelationPayType { get; set; }

        public bool IsFixedAmount { get; set; }


    }
}
