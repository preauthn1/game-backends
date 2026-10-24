using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class WagetFetchJobParameter
    {
        public DateTime? BeginDateTW { get; set; }

        public DateTime? EndDateTW { get; set; }

        public int VendorConfigID { get; set; }

        public bool IsAsync { get; set; }

        public int? SplitMinutes { get; set; }

        public string GameCode { get; set; }


    }



}
