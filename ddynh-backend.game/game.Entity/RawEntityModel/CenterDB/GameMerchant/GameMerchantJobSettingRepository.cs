using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class GameMerchantJobSettingRawEntity
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// GameMerchantJobTypeEnum
        /// </summary>
        public int JobType { get; set; }
        public bool IsEnable { get; set; }
        public string Memo { get; set; }
        public DateTime? LastExecutedTime { get; set; }
        public int JobIntervalSeconds { get; set; }
        public string JobAPIHost { get; set; }
        public string JobAPIUrl { get; set; }
        public DateTime JobNextStarttTime { get; set; }
        public string DebugInfo { get; set; }
        public int PlatformID { get; set; }
        public int GameMerchantID { get; set; }

    }
}
