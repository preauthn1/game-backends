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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class PlayerTokenLogoutService : BaseAdminSysService, IPlayerTokenLogoutRefService
    {
        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }
      
        public PlayerGameTokenLogRepository PlayerGameTokenLogRepository { get; set; }


        public void ResetAllSessionToken()
        {
            var emptyFilter = new ResetTokenParameter
            {
                IsClearAll = true
            };

            ResetSessionTokenByParameter(emptyFilter);
        }


        public void ResetMerchantPlayerSessionToken(int merchantID)
        {
            var filter = new ResetTokenParameter
            {
                MerchantID = merchantID
            };

            ResetSessionTokenByParameter(filter);
        }


        public void ResetPlayerSessionToken(int playerID)
        {
            var filter = new ResetTokenParameter
            {
                PlayerID = playerID
            };

            ResetSessionTokenByParameter(filter);
        }


        public void ResetSessionTokenByParameter(ResetTokenParameter filter)
        {
            VendorSessionTokenRepository.ResetAllSessionToken(filter); //#001
            LobbyTokenLogRepository.ResetAllToken(filter);  //#改寫
            PlayerGameTokenLogRepository.ResetAllToken(filter);
                        
            if(filter.PlayerID.HasValue)
            {
                //repositroy裡面有實作了
            }
            else
            {
                //*指定玩家ID時。請勿呼叫 RemoveCacheAll 有客戶端的API會使用到此邏輯
                CacheHelper.RemoveCacheAll();
            }
        }


    }

}
