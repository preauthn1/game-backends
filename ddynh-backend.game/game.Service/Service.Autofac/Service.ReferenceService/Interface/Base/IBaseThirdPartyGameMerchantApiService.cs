using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{

    public interface IBaseThirdPartyGameMerchantApiService
    {

        ThirdPartyGameApiWalletAmountResultModel GetWalletAmount(ThirdPartyGameMerchantApiWalletAmountRequestModel model);

        ThirdPartyGameApiWalletTransferResultModel TransferMoneyToGameWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model);

        ThirdPartyGameApiWalletTransferResultModel TransferMoneyToCenterWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model);

        ThirdPartyGameMerchantApiCreateAccountResultModel CreateUserGameAccount(ThirdPartyGameApiAccountCreateResultModel model);

        ThirdPartyGameApiAccountExistResultModel IsAccountExist(ThirdPartyGameApiCommonParameterModel model);

        ThirdPartyGameApiLaunchResultModel GetGameLaunchResult(ThirdPartyGameApiLaunchRequestModel model);

    }
}
