using FW.Core.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PromotionEventEntity
    {

        [Key]
        public int EventId { get; set; }
        
        public int PlatformID { get; set; }

        public string Title { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime EventEndTime { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }      

    }




}
