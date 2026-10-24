using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class UserGameLaunchSummaryRawEntity
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int GameID { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastLaunchTime { get; set; }
        public int LaunchTimes { get; set; }


    }
}
