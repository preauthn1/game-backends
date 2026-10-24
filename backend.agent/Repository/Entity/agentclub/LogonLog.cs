using System;

namespace Repository.Entity.agentclub
{
    public class LogonLog
    {
        public long LogID { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
