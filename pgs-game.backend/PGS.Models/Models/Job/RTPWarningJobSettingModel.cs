using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class RTPWarningJobSettingModel
    {
        public int DailyRoundMustLargeThan { get; set; } 
        
        /// <summary>                                                         
        /// 連續N日RTP「低於」數值                                                         
        /// </summary>
        public int WarningContinueLowerRTP { get; set; }

        /// <summary>
        /// 平均RTP「低於」數值 會發通知
        /// </summary>
        public int WarningLowerAvgRTPValue { get; set; }

        /// <summary>
        /// 連續N日RTP「高於」數值  
        /// </summary>
        public int WarningContinueRTP { get; set; }

        /// <summary>
        /// 平均RTP「高於」數值 會發通知
        /// </summary>
        public int WarningAvgRTPValue { get; set; }

        /// <summary>
        /// 連續幾天
        /// </summary>
        public int ContinueDays { get; set; }
    }



}
