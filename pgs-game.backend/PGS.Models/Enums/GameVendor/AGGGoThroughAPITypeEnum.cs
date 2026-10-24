using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum AGGGoThroughAPITypeEnum
    {
        [Description("command/v2/game/freespin-campaign/list")]
        FreespinSpin_List,

        [Description("command/v2/game/freespin-campaign/set")]
        FreespinSpin_Set,

        [Description("command/v2/game/freespin-campaign/disable")]
        FreespinSpin_Disable,

        [Description("command/v2/game/freespin-campaign/status")]
        FreespinSpin_Status,

        [Description("command/v2/game/freespin-campaign/record")]
        FreespinSpin_Record,

        [Description("command/v2/game/freespin-campaign/summary")]
        FreespinSpin_Summary,
    }


}
