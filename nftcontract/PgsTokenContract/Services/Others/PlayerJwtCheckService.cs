using Microsoft.AspNetCore.Http;
using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Models;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PgsTokenContract.Services
{
    public class PlayerJwtCheckService
    {
        public PlayerTokenJwtModel CheckAndParsingJwt(string jwtToken)
        {
            //jwt檢查包含 檢查偽造、過期、解析失敗
            if (string.IsNullOrWhiteSpace(jwtToken))
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            PlayerTokenJwtModel convertUserModel = MyJwtHelper.GetPlayerModelFromJwtString(jwtToken);

            if (convertUserModel == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            var now = DateTimeHelper.GetCurrentTimeTimestamp();

            if (convertUserModel.TokenExpiredTime <= now)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            return convertUserModel;
        }
    }
}
