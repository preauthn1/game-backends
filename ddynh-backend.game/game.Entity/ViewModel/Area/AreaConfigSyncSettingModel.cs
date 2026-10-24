using FW.Core.Helper;

namespace game.Entity.ViewModels
{

    /// <summary>
    /// 渠道同步設定，某幾個比較嚴重高風險的欄位可以設定不同步
    /// </summary>
    public class AreaConfigSyncOptionalSettingModel
    {        
        
        public bool IsSyncAgentSetting { get; set; }

        public bool IsSyncDownloadUrl { get; set; }

        public bool IsSyncHallStyle { get; set; }

    }
}
