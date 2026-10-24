using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language.DataAnnotations
{
    public class ResStringLengthAttribute : StringLengthAttribute
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="maximumLength"></param>
        public ResStringLengthAttribute(int maximumLength)
            :base(maximumLength)
        { }

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

        protected override System.ComponentModel.DataAnnotations.ValidationResult IsValid(object value, System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            this.SetValidErrorMessage(ErrorMessageResourceCatalog, ErrorMessageResourceKey, validationContext);

            return base.IsValid(value, validationContext);
        }
    }
}
