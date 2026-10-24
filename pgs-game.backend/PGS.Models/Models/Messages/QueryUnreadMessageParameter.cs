using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class QueryUnreadMessageParameter
    {
        public int PageSize { get; set; }

        public bool IsQueryTotalCount { get; set; }

        public bool IsQueryUnreadOnly { get; set; }

    }

}
