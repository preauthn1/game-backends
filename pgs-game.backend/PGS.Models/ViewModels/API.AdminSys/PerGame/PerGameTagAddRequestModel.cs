using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PerGameTagAddRequestModel
    {
        public long PerGameDetailLogID { get; set; }
             
        /// <summary>
        /// SystemTagEnum
        /// </summary>
        public int TagID { get; set; }
    }

    public class PerGameTagBatchAddRequestModel
    {
        public List<string> TransactionNoList { get; set; }
       
        public int TagID { get; set; }
    }



}
