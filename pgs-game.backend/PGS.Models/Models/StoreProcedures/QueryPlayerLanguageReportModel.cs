using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class QueryPlayerLanguageReportModel
    {
        public string LanguageCode { get; set; }

        public int TotalCount { get; set; }
    }
}
