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

    public class PlayerVendorService : BaseVendorService, IPlayerVendorService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public PlayerFakeCreateRecordRepository PlayerFakeCreateRecordRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public IPlayerCreateRefService PlayerCreateRefService { get; set; }

        public IPlayerTokenLogoutRefService PlayerTokenLogoutRefService { get; set; }


        public PlayerVendorViewModel IsAccountExist(QueryPlayerAccountExistRequestModel filter)
        {
            filter.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            string selectColumns = " PlayerID , MerchantID , PlayerName,  CreateTime , Status "; //輕量化搜尋
            
            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(filter.PlayerName , filter.MerchantID , selectColumns);

            if(queryPlayer == null)
            {
                //如果商戶有禁止建帳號
                if(IsForbiddenToCreateAccountByVendorAPI(filter.MerchantID))
                {
                    var queryFakeData = PlayerFakeCreateRecordRepository.GetByPlayerNameAndMerchantID(filter.PlayerName, filter.MerchantID);
                    return new PlayerVendorViewModel
                    {
                        IsExist = queryFakeData != null,
                        CreateTime = queryFakeData?.CreateTime
                    };
                }
            }

            return new PlayerVendorViewModel
            {
                IsExist = queryPlayer != null,
                CreateTime = queryPlayer?.CreateTime
            };
        }



        public PlayerCreateResultVenodrApiViewModel CreateNewPlayer(CreatePlayerAccountExistRequestModel model)
        {
            var queryMerchant =  this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            if(queryMerchant.WalletMode == (int)MerchantWalletModeEnum.SingleWalletMode)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Seamless"));
            }

            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText().Rule_IsValidAccountFormat();

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var resultPlayerID = PlayerCreateRefService.CreatePlayerForMerchantRequest(new PlayerCreateParameterModel
            {
                Currency = queryMerchant.Currency,
                MerchantID = model.MerchantID,
                PlayerName = model.PlayerName,
                IsSkipExceptionWhenDataDuplicate = true
            });

            return new PlayerCreateResultVenodrApiViewModel
            {

            };
        }


        public PlayerLogoutResultViewModel ForcePlayerLogout(PlayerLogoutRequestModel model)
        {
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            var queryMerchant = this.GetMerchantAndCheckIsValid(model.MerchantID, model.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerName, model.MerchantID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            PlayerTokenLogoutRefService.ResetSessionTokenByParameter(new ResetTokenParameter
            {                
                PlayerID = queryPlayer.PlayerID,
                IsRemoveVendorSessionTokenCache = true,
            });

            PlayerTagMappingRepository.CreatePlayerBehaviorEvent(queryPlayer.PlayerID, $"trigger PlayerLogout (强制登出)");

            return new PlayerLogoutResultViewModel
            {
                PlayerName = model.PlayerName
            };
        }


    }
}
