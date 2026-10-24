using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.BL.Services
{

    /// <summary>
    /// AGG/IDG API轉換層
    /// </summary>
    public class AGGGoThroughService : BaseVendorService
    {

        public AGGIntegrationService AGGIntegrationService = new AGGIntegrationService();

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }


        public AGGGoThroughReqeustParameter _params = new AGGGoThroughReqeustParameter();



        public object FreespinSpin_List(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            FreeSpinCampaignListResponse result = (FreeSpinCampaignListResponse)ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_List);

            SetGameID(result?.freeSpinCampaigns?.SelectMany(z=> z.gameList));

            return result;
        }


        public object FreespinSpin_Set(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            FreeSpinCampaignSetResponse result = (FreeSpinCampaignSetResponse)ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_Set);

            SetGameID(result?.freeSpinCampaign?.gameList);

            return result;
        }


        public object FreespinSpin_Disable(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            var result = ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_Disable); 
            return result;
        }

        public object FreespinSpin_Status(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            AGGFreeSpinStatusResponse result = (AGGFreeSpinStatusResponse)ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_Status);
            return result;
        }

        public object FreespinSpin_Record(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            AGGFreeSpinRecordResponse query = (AGGFreeSpinRecordResponse)ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_Record);
            return query;
        }

        public object FreespinSpin_Summary(Dictionary<string, object> keyValues)
        {
            OverrideColumnValue(keyValues);

            var result = ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum.FreespinSpin_Summary);
            return result;
        }


        /// <summary>
        /// 欄位轉換
        /// </summary>
        private void OverrideColumnValue(Dictionary<string, object> keyValues)
        {
            List<string> removeKeys = new List<string>();

            foreach (var key in keyValues.Keys)
            {
                string keyLower = key.ToLower();

                if (keyLower == "merchantid")
                {
                    _params.CommonParams.MerchantID = Convert.ToInt32(keyValues[key]);
                    removeKeys.Add(key);
                }
                else if (keyLower == "merchantsecretkey")
                {
                    _params.CommonParams.MerchantSecretKey = (string)(keyValues[key]);
                    removeKeys.Add(key);
                }                
                else if (keyLower == "playername")
                {
                    _params.CommonParams.PlayerName = (string)(keyValues[key]);
                    removeKeys.Add(key);
                }
                else if (keyLower == "gameid")
                {
                    _params.CommonParams.GameID = Convert.ToInt32(keyValues[key]);
                    removeKeys.Add(key);
                }
                else if (keyLower == "gameids")
                {
                    _params.CommonParams.GameIDs = (List<int>)keyValues[key];
                    removeKeys.Add(key);
                }
            }

            foreach (var eachKey in removeKeys)
            {
                keyValues.Remove(eachKey);
            }

            _params.KeyValues = keyValues;


            //所有API都會需要merchantID
            QueryMerchantAndSetProperty();

            return;
        }



        private void SetGameID(IEnumerable<AGGFreeSpinGameList> games)
        {
            if (games.IsEmptyOrNull())
                return;

            var queryAGGGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),
            });

            foreach(var item in games)
            {
                var matchData = queryAGGGames.FirstOrDefault(z => z.ThirdPartyGameCode == item.gameId);

                if(matchData != null)
                {
                    item.gameId = matchData.GameID.ToString();
                }
            }
        }



        private object ValidateParameterAndCallAPI(AGGGoThroughAPITypeEnum apiType)
        {
            var commonParam = _params.CommonParams;

            switch (apiType)
            {
                case AGGGoThroughAPITypeEnum.FreespinSpin_List:
                    break;

                case AGGGoThroughAPITypeEnum.FreespinSpin_Set:
                    commonParam.RuleFor(x => x.GameIDs).Rule_IsNotEmptyCollection();
                    QueryGameListAndSetProperty();
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            var apiResult = AGGIntegrationService.RequestGoThroughAPIByType(apiType, _params);
            return apiResult;
        }


        private void QueryMerchantAndSetProperty()
        {
            var commonParam = _params.CommonParams;

            commonParam.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            commonParam.RuleFor(x => x.MerchantSecretKey).Rule_NotEmptyText();

            var queryMerchant = GetMerchantAndCheckIsValid(commonParam.MerchantID, commonParam.MerchantSecretKey);
            _params.CommonParams.Currency = queryMerchant.Currency;
        }



        private void QueryPlayerAndSetProperty()
        {
            var commonParam = _params.CommonParams;

            if (string.IsNullOrWhiteSpace(commonParam.PlayerName))
                return;

            commonParam.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(commonParam.PlayerName, commonParam.MerchantID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            _params.KeyValues.Add("userId", queryPlayer.PlayerID.ToString());
        }

        private void QueryPlayerListAndSetProperty()
        {
            var commonParam = _params.CommonParams;

            if (string.IsNullOrWhiteSpace(commonParam.PlayerName))
                return;

            commonParam.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(commonParam.PlayerName, commonParam.MerchantID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            _params.KeyValues.Add("userIdList", queryPlayer.PlayerID.ToString());
        }


        private void QueryGameAndSetProperty()
        {
            var commonParam = _params.CommonParams;

            if (commonParam.GameID <= 0)
                return;

            var queryGame = GameListRepository.GetById(commonParam.GameID);

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            _params.KeyValues.Add("gameId", queryGame.ThirdPartyGameCode);
        }

        private void QueryGameListAndSetProperty()
        {
            var commonParam = _params.CommonParams;

            if (commonParam.GameIDs.IsEmptyOrNull())
                return;

            var queryGames = GameListRepository.GetByIds(commonParam.GameIDs);

            if (queryGames.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundGame);

            _params.KeyValues.Add("gameList", queryGames.Select(x=> x.ThirdPartyGameCode).ToList());
        }


       

    }

}