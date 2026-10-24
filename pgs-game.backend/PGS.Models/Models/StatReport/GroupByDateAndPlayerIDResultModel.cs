using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class GroupByDateAndPlayerIDResultModel
    {
        public DateTime Date { get; set; }

        public int PlayerID { get; set; }

    }
}
