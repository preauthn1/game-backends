using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language.DataAnnotations
{
    public class ResMaxLengthAttribute : MaxLengthAttribute
    {
        public string ErrorMessageResourceCatalog
        {
            get;
            set;
        }

        public string ErrorMessageResourceKey
        {
            get;
            set;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            this.SetValidErrorMessage(ErrorMessageResourceCatalog, ErrorMessageResourceKey, validationContext);

            return base.IsValid(value, validationContext);
        }
    }
}
