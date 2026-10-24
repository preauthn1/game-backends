using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Utility.ThirdParty
{
    public enum SlackChannelEnum
    {        
        [SlackTeam("target")]
        n_api_payment,

        [SlackTeam("target")]
        n_api_gameapi,

        [SlackTeam("target")]
        n_dd_api_game_merchant,

        [SlackTeam("target")]
        n_dd_job_third_party,

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
