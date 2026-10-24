using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Utility.ThirdParty
{
    public enum SlackChannelEnum
    {        

        [SlackTeam("winwings")]
        n_gitlab_hook,

        [SlackTeam("winwings")]
        backend_mr,

        [SlackTeam("winwings")]
        backend_release,

        [SlackTeam("winwings")]
        backend_release_prod,
    }

    /// <summary>
    /// 純粹拿來標記 是屬於哪個組織下的頻道 
    /// 跟程式碼運作無關
    /// </summary>
    public class SlackTeam : Attribute
    {
        public SlackTeam(string teamName)
        {

        }
    }
    

}
