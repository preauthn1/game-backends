using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class PlayerMessageExtraInfoModel
    {
        public long MessageID { get; set; }
        public int AssignType { get; set; }
        public int ContentTemplateID { get; set; }
        public int Status { get; set; }
        public string ParameterJson { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public bool IsRead { get; set; }

    }

}
