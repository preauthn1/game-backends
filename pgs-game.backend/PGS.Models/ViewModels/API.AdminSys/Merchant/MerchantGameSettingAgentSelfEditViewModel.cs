using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System.Collections.Generic;

namespace target.Models.ViewModels
{


    public class MerchantGameSettingAgentSelfEditViewModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public List<int> GameIDs_Slots { get; set; }

        public List<int> GameIDs_Live { get; set; }

        /// <summary>
        /// 同步給以下代理
        /// </summary>
        public List<int> SyncSubAgentIDs { get; set; }

        public bool IsDisableAllEditGameOpenFlag { get; set; }

        public bool IsPermissionCanEnableOnly { get; set; }

        public string ChildAgentHintMsg { get; set; }

        public List<DropdownValueViewModel> ChildAgentDropdowns { get; set; }

        public List<MerchantGameSettingAgentSelfEditItemViewModel> GameSettingItems { get; set; }

    }


    public class MerchantGameSettingAgentSelfEditItemViewModel
    {
        public int GameID { get; set; }

        public string GameName { get; set; }

        public int GameType { get; set; }

        public string GameTypeText { get; set; }

        public bool MerchantGameEnableFlag { get; set; }

        public bool SubAgentDefaultEnableGameFlag { get; set; }

        public bool IsLobbyVisibleFlag { get; set; }

        /// <summary>
        /// 是否GameList這張表 狀態是關閉
        /// </summary>
        public bool IsGameListDisable { get; set; }


        /// <summary>
        /// 一般原因禁止編輯 (權限不足/自己編輯自己)
        /// </summary>
        public bool IsGameOpenFlagDisableEdit { get; set; }


        /// <summary>
        /// 因為特殊原因 (獨佔或其他) 禁止編輯
        /// </summary>
        public bool IsSpecialReasonDisableEdit { get; set; }


    }

    public class MerchantDefaultEnableGamesForSubAgnetViewModel
    {
        public int WalletMode { get; set; }

        public int IMChannelID { get; set; }

        public bool IsDisableIMChannelAutoComplete { get; set; }

        public List<DropdownValueViewModel> AvailableGamesDropdown { get; set; }

        public List<int> DefaultEnableGameIDs { get; set; }

    }

    public class MerchantGameSettingAgentSelfEditEntityUpdateModel
    {
        public int MerchantID { get; set; }

        public List<MerchantGameSettingEntity> MerchantGameSettingEntityItems { get; set; }

        public List<MerchantGameSettingSelfConfigEntity> MerchantGameSettingSelfConfigEntityItems { get; set; }

    }





}
