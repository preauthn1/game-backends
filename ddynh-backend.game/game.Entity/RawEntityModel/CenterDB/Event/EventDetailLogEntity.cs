using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{
    public class EventDetailLogEntity
    {
        [Key]
        public long ID { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public DateTime CreateTime { get; set; }
        public long GiftCash { get; set; }
        public string IPAddress { get; set; }
        public int EventSettingID { get; set; }
    }

}
