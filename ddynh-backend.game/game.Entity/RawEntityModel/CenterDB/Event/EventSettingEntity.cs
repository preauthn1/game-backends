using FW.Core.Repository;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{

    public class EventSettingEntity
    {
        [Key]
        public int ID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Status { get; set; }
        public long GiftCash { get; set; }
        public string Updater { get; set; }
        public DateTime UpdateTime { get; set; }
        public int EventType { get; set; }
        public int PlatformID { get; set; }
        public string Memo { get; set; }
        public string Tips { get; set; }
    }

}
