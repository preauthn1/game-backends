using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;

namespace target.BL.Services
{

    public class ViewPermissionCheckService : BaseAdminSysService, IViewPermissionCheckService
    {

        public UserRepository UserRepository { get; set; }


        public PlayerRepository PlayerRepository { get; set; }


        public void CheckViewPlayerPermission(int currentUserID, int targerPlayerID)
        {
            base.CheckViewPlayerPermission(currentUserID, targerPlayerID);
        }

        public void CheckViewMerchantPermission(int currentUserID, int targetMerchantID)
        {
            base.CheckViewMerchantPermission(currentUserID, targetMerchantID);
        }

        public override void CheckViewUserPermission(int currentUserID , int targetUserID)
        {
            base.CheckViewUserPermission(currentUserID, targetUserID);
        }


    }
}
