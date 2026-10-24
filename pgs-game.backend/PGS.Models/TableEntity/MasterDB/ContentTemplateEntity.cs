using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class ContentTemplateEntity
    {
        [Key]
        public int ContentTemplateID { get; set; }
        public Guid ContentTemplateGuid { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public string CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
