using System;
using System.Collections.Generic;
using game.Entity.Game;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IPlayerSuspiciousCheckService
    {
     
        PlayerSuspiciousCheckResultViewModel CheckPlayerDataIsSuspicious(int userID, LazyCommonRequestModel filter);

        PlayerLiveCasinoSuspiciousCheckResultViewModel CheckPlayerLiveCasinoGameSuspicious(int userID, LazyCommonRequestModel filter);

     
    }
}
