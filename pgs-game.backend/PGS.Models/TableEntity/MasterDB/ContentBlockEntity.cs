using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class ContentBlockEntity
    {
        [Key]
        public int ContentBlockID { get; set; }
        public string LanguageCode { get; set; }
        public Guid? OwnerGuid { get; set; }
        public string Memo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrlTop { get; set; }
        public string ImageUrlMiddle { get; set; }
        public string ImageUrlBottom { get; set; }
        public int LinkType { get; set; }
        public string LinkReference { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
