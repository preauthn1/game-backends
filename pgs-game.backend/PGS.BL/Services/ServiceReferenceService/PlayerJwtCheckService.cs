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
using target.DA.Repository;
using target.Utility;
using target.Utility.Helper;
using target.Models.ViewModels.Base;
using Lck.Utility.Extensions.Helper;
using target.Models;

namespace target.BL.Services
{

    public class PlayerJwtCheckService : BaseService, IPlayerJwtCheckService
    {

        public PlayerRepository PlayerRepository { get; set; }

        public PlayerGameTokenLogRepository PlayerGameTokenLogRepository { get; set; }

        public LobbyTokenLogRepository LobbyTokenLogRepository { get; set; }


        /// <summary>
        /// 提供給 AuthorizeFilter呼叫，檢查DB紀錄的token是否已過期
        /// </summary>
        public bool CheckIsTokenExist(string jwtHash)
        {
            //MSTest測試環境先不需要後踢前
            if (SettingHelper.IsMSTest)
                return true;

            //v1版的寫法
            //var queryPlayer = PlayerRepository.GetByTokenAndCache(jwtHash);

            //if (queryPlayer == null)
            //    throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            //v2版的寫法
            var queryToken = [REDACTED];

            if (queryToken == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            if (queryToken.ExpiredTime < DateTimeHelper.GetCurrentTime())
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            return true;
        }


        /// <summary>
        /// 提供給 AuthorizeFilter呼叫，檢查DB紀錄的token是否已過期
        /// </summary>
        public bool CheckIsLobbyTokenExist(string tokenRawText)
        {
            CheckLobbyTokenExistAndReturnQueryData(tokenRawText);
            return true;
        }


        /// <summary>
        /// 解析lobbyToken
        /// </summary>
        public LobbyTokenParsingResultModel ParsingLobbyToken(string tokenRawText)
        {
            var queryExistToken = [REDACTED];

            string tokenRawJwt = queryExistToken.RawJwtToken;

            var parseModel = MyJwtHelper.GetPlayerModelFromJwtString(tokenRawJwt);

            if (parseModel == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            // CheckLobbyTokenExistAndReturnQueryData 已經有檢查expired時間了         
            var parsingRequestInfo = ParsingRawRequestInfo(queryExistToken.RawRequestInfo);

            HeaderValueHelper.AppendLogInfoUserIdentity(queryExistToken.PlayerID);
            HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_IdentityMemo, queryExistToken.MerchantID.ToString());

            return new LobbyTokenParsingResultModel
            {
                ParsingTokenModel = parseModel,
                ParsingRawRequest = parsingRequestInfo,
                QueryId = queryExistToken.Id,
            };
        }


        private GameLaunchRequestModel ParsingRawRequestInfo(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return null;

            try
            {
                return rawJson.JsonConvertToModel<GameLaunchRequestModel>();
            }
            catch
            {

            }

            return null;
        }

        private LobbyTokenLogEntity CheckLobbyTokenExistAndReturnQueryData(string tokenRawText)
        {
            if (string.IsNullOrWhiteSpace(tokenRawText))
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            //現在版本的token一律都是Guid格式
            if (!tokenRawText.IsGuid())
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            Guid guid = Guid.Parse(tokenRawText);

            string cacheKey = CacheKey.Lobby_PlayerJwtTokenByGuid(guid);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var query = LobbyTokenLogRepository.GetByGuid(guid);
                   return query;
               });

            LobbyTokenLogEntity queryToken = [REDACTED];

            if (queryToken == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_NotValid);

            var now = DateTimeHelper.GetCurrentTime();

            if (now >= queryToken.ExpiredTime)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            return queryToken;
        }

    }
}
