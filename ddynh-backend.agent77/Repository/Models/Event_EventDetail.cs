using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Event_EventDetail
    {
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime EventStartDate { get; set; }
        public DateTime EventEndDate { get; set; }
        public int Status { get; set; }
        public long? LngValue1 { get; set; }
        public long? LngValue2 { get; set; }
        public long? LngValue3 { get; set; }
        public long? LngValue4 { get; set; }
        public long? LngValue5 { get; set; }
        public long? LngValue6 { get; set; }
        public long? LngValue7 { get; set; }
        public long? LngValue8 { get; set; }
        public long? LngValue9 { get; set; }
        public long? LngValue10 { get; set; }    
    }
}
