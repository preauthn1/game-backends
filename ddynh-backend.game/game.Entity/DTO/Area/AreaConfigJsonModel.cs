using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{


    /// <summary>
    /// 這個模型對應 area資料表的 areaConfig欄位 (裡面存json)
    /// </summary>
    public class AreaConfigJsonModel
    {
        public AreaConfigSystemConfig systemConfig { get; set; }
    }


    public class AreaConfigJsonModelTemp 
    {
        public int AreaID { get; set; }

        public AreaConfigJsonModel AreaConfigSetting { get; set; }

    }



    public class PayConfig
    {
        public bool UIFlagPay { get; set; }
        public bool UIFlagAgent { get; set; }
        public bool UIFlagApplyAgent { get; set; }
        public bool UIFlagJiuLiao { get; set; }
        public bool UIFlagShanFu { get; set; }
        public bool UIClub2AgentMode1 { get; set; }
        public bool UIClub2AgentMode2 { get; set; }
        public bool UIClub2AgentMode3 { get; set; }
        public bool UIClub2AgentMode4 { get; set; }
        public bool UIClub2AgentMode5 { get; set; }
        public bool UIClub2AgentMode6 { get; set; }
        public bool ConvertAndOr { get; set; }
        public bool AllGameAndOr { get; set; }
        public bool VIPAgentAndOr { get; set; }
        public bool CSAndOr { get; set; }
    }

    public class Info
    {
        public int PlatformID { get; set; }
        public int SiteID { get; set; }
        public int Agent { get; set; }
        public string Template { get; set; }
        public int FlagExchage { get; set; }
        public int ExchangeFontType { get; set; }
        public int Club2Agent { get; set; }
        public int ShowCaiJinchi { get; set; }
        public int FlagWebViewIcon1 { get; set; }
        public int WebViewIcon1Type { get; set; }
        public string WebViewIcon1Url { get; set; }
        public int FlagCSThirdparty { get; set; }
        public string CSThirdpartyUrl { get; set; }
    }

    public class JiuLiao
    {
        public string DownloadUrl { get; set; }
    }

    public class AreaConfigSystemConfig
    {
        public PayConfig payConfig { get; set; }
        public Info Info { get; set; }
        public JiuLiao JiuLiao { get; set; }
    }


}
