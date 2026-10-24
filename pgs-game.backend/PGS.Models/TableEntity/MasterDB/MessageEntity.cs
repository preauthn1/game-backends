using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MessageEntity
    {
        [Key]
        public long MessageID { get; set; }
        public int AssignType { get; set; }
        public int ContentTemplateID { get; set; }
        public int Status { get; set; }
        public string ParameterJson { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
