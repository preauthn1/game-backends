namespace game.Entity.Game
{
    public class AreaConfig
    {
        public System systemConfig { get; set; }

        public class System
        {
            public PayConfig payConfig { get; set; }
            public Info Info { get; set; }
            public JiuLiao JiuLiao { get; set; }
        }

        public class Info
        {
            public string BundleID { get; set; }
            public string AppName { get; set; }
            public string AppleID { get; set; }
            public int ServerOption { get; set; }
            public int PlatformID { get; set; }
            public int SiteID { get; set; }
            public int StaticSiteID { get; set; }
            public int Agent { get; set; }
            public string Template { get; set; }
            public int FlagExchage { get; set; }
            public int ExchangeFontType { get; set; }
            public int Club2Agent { get; set; }
            public int ShowCaiJinchi { get; set; }
        }

        public class JiuLiao
        {
            public string JiuLiaoId { get; set; }
            public string JiuLiaoSecrety { get; set; }
            public string JiuLiaoName { get; set; }
            public string DownloadUrl { get; set; }
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
    }
}
