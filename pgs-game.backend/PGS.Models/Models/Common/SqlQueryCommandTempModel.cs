using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class SqlQueryCommandTempModel<T>
    {
        public string SqlWithParameterValue { get; set; }

        public List<T> ListResult { get; set; }
    }
}
