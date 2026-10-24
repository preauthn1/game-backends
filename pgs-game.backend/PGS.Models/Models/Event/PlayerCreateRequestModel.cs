using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class EventProcessModel
    {
        public EventProcessModel()
        {
            this.ExtraParameter = new ExtraEventProcessParameter();
        }


        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public bool isThrowException { get; set; }

        public bool IsReturnForDebug { get; set; }

        /// <summary>
        /// 特殊情況、不同案例  才會帶入的參數
        /// </summary>
        public ExtraEventProcessParameter ExtraParameter { get; set; }

    }


    public class ExtraEventProcessParameter
    {
        public string BetRecordNoPGS { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public DateTime BetTime { get; set; }

        public DateTime? SettleTime { get; set; }
    }




}
