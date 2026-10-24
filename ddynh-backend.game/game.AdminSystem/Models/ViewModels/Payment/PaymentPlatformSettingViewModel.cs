using game.Entity.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
   
    public class PaymentPlatformSettingViewModel
    {

        public List<PayTypeManagementAmountListModel> SameTypePaymentGateways { get; set; }

        public PayTypeManagementAmountListModel CurrentEditPaymentGateway { get; set; }
        
        public int PlatformID { get; set; }

    }

    public class PaymentPlatformSettingUpdateAllViewModel
    {
        public List<PayTypeManagementAmountListModel> SameTypePaymentGateways { get; set; }

        public int PlatformID { get; set; }

        public int GWID { get; set; }

        public string GWName { get; set; }


    }






}