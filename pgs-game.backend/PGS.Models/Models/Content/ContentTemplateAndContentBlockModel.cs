using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Models.Table;

namespace target.Models.Models
{
    public class ContentTemplateAndContentBlockModel
    {
        public int ContentTemplateID { get; set; }

        public Guid ContentTemplateGuid { get; set; }

        public string Memo { get; set; }

        public List<ContentBlockEntity> ContentBlocks { get; set; }

    }
}
