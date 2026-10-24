using game.Entity.Event;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{

    public class ConvertGatewayAdminUpdateViewModel : ConvertGatewayEntity
    {

        public bool IsUpdatePage { get; set; }

    }

    public class ConvertGatewayUpdateBankSettingsViewModel 
    {

        public int GWID { get; set; }

        public string GWName { get; set; }

        public List<ConvertGatewayUpdateBankSettingsItemsViewModel> SettingItems { get; set; }

    }

    public class ConvertGatewayUpdateBankSettingsItemsViewModel : ConvertGatewayBankSettingEntity
    {

        public string BankName { get; set; }

        public bool IsChecked { get; set; }
    }



    /// <summary>
    /// 顯示資料用
    /// </summary>
    public class ConvertGatewayDisplayViewModel : ConvertGatewayEntity
    {
        public string ConvertTypeText { get; set; }
        public string StatusText { get; set; }
        public string CreateTimeText { get; set; }
        public string UpdareTimeText { get; set; }
        public int AvailableBankCount { get; set; }

    }


    public class QueryConvertGatewayJQueryRequestModel : JQueryDataTablePagingModel
    {
        public int? PlatformID { get; set; }


        public string PlatformIDsText { get; set; }

    }



}