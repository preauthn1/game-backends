using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{

    public class JobTriggerFetchBetRecordParameterModel
    {
        public int PlatformID { get; set; }

        public DateTime? BeginTimeUTC { get; set; }

        public DateTime? EndTimeUTC { get; set; }


        /// <summary>
        /// 若有指定，只會寫入指定的注單號 (特定遊戲才會處理此邏輯)
        /// </summary>
        public List<string> SpecificBetRecordNo { get; set; }


    }

    public class JobTriggerFetchBetRecordSimpleParameterModel
    {
        public int PlatformID { get; set; }
        
    }




}
