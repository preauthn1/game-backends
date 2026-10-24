using FW.Core.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PromotionEventAreaMappingEntity
    {

        [Key]
        public int MappingID { get; set; }
        
        public int EventID { get; set; }

        public int AreaID { get; set; }


    }
}
