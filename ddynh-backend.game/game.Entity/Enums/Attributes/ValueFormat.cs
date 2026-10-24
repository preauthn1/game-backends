using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    
    public class ValueFormat : System.Attribute
    {       
        public ValueFormatRestrictionEnum Format { get; set; }

        public ValueFormat (ValueFormatRestrictionEnum value)
        {
            this.Format = value;
        }
    }
}
