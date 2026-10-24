using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum AgentAggregativeTypeEnum
    {
        IncludeMerchantAndChildMerchants = 1,

        OnlyMerchant = 7,

        OnlyChildMerchants = 9,
    }
}
