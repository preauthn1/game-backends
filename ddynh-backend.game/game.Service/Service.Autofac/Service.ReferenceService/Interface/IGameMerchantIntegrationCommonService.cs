using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{

    public interface IGameMerchantIntegrationCommonService
    {
        void DoTransferToGameWalletProcess(int gameMerchantID, ThirdPartyGameMerchantApiWalletTransferRequestModel model);

        void DoTransferToCenterWalletProcess(int gameMerchantID, ThirdPartyGameMerchantApiWalletTransferRequestModel model);

        void CheckGameMerchantIsValid(int gameMerchantID, int platformID);
    }
}
