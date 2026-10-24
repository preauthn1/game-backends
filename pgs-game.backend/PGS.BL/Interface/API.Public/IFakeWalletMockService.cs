using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IFakeWalletMockService
    {
        (int PlayerID, string SessionTokenPGS) GetPlayerIDByPlatfromSessionToken(string sessionToken);

        (int PlayerID, string SessionTokenPGS) GetPlayerIDByW88Token(string w88Token);

        decimal GetFakeWalletBalance(int playerID);

        decimal UpdateFakeWalletAmount(int playerID, decimal updateAmount);

    }
}
