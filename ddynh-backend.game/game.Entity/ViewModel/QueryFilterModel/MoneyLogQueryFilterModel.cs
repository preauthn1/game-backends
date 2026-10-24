using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class MoneyLogQueryFilterModel
    {
        public MoneyLogQueryFilterModel()
        {
            this.PageIndex = 1;
            this.PageSize = 20;
        }


        public int? UserID { get; set; }
        public int? SourceTypeGroup { get; set; }
        public Int16? SourceType { get; set; }
        public string IpAddress { get; set; }
        public string Memo { get; set; }

        public DateTime? BeginTimeConvert
        {
            get
            {
                return TryParseDateTime(BeginTime);
            }
        }

        public DateTime? EndTimeConvert
        {

            get
            {
                return TryParseDateTime(EndTime);
            }
        }


        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public int? AreaID { get; set; }

        public int? PlatformId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int? AdminID { get; set; }


        private DateTime? TryParseDateTime(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            DateTime result;

            bool isOK = DateTime.TryParse(dateText, out result);

            if (isOK)
                return result;

            return null;
        }





    }

}
