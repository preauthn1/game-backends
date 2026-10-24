using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MerchantSelectTemplateEntity
    {

        [Key]
        public int TemplateID { get; set; }

        /// <summary>
        /// DataCategoryEnum
        /// </summary>
        public int CategoryID { get; set; }

        public int ValueType { get; set; }

        public int Status { get; set; }

        public string DisplayName { get; set; }

        public string Memo { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }


    }
}
