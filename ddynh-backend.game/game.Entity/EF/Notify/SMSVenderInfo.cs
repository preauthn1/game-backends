using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Notify
{
    public class SMSVenderInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int SMSSettingId { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformId { get; set; }

        /// <summary>
        /// 短信商ID, 對應ServerInfo.SMSConfig.ID
        /// </summary>
        public int SMSId { get; set; }

        /// <summary>
        /// 短信商配置
        /// </summary>
        public string SMSInfo { get; set; }
		
        /// <summary>
        /// 權重
        /// </summary>
        public int Sort { get; set; }
		
        /// <summary>
        /// 狀態
        /// </summary>
        public int Status { get; set; }
    }
}
