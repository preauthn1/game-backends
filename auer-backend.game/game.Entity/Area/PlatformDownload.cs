using System;

namespace game.Entity.Area
{
    public class PlatformDownload
    {
        public string Name { get; set; }
        public int PlatformID { get; set; }
        public string IOS { get; set; }
        public string Android { get; set; }
        public string Official { get; set; }
        public byte IOSTerminate { get; set; }
        public string IOSMemo { get; set; }
        public byte Correct { get; set; }
        public DateTime UpdateTime  { get; set; }
        public string Memo { get; set; }
    }
}
