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
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class AdminSysSimpleUpdateService : BaseAdminSysService, IAdminSysSimpleUpdateService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public PlayerCustomizedSettingRepository PlayerCustomizedSettingRepository { get; set; }


        /// <summary>
        /// 很簡單的表單就直接用通用的API去寫了 省得麻煩
        /// </summary>
        public CRUDResultModel SimpleUpdateByParameter(int userID, SimpleUdpateRequestModel postModel)
        {
            if (postModel.UpdatePlayerMemo != null)
            {
                UpdatePlayerMemo(userID , postModel.UpdatePlayerMemo);
            }

            return new CRUDResultModel();
        }


        private void UpdatePlayerMemo(int userID , PlayerMemoUpdateColumnRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.Memo).Rule_NotEmptyText();

            var queryPlayer = PlayerRepository.GetById(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            CheckViewPlayerPermission(userID , model.PlayerID);

            string myMemoText = model.Memo.Truncate(50);

            PlayerCustomizedSettingRepository.CreateOrUpdate(queryPlayer.PlayerID, PlayerCustomizedSettingKeyEnum.Player_Memo, myMemoText);

            PlayerTagMappingRepository.CreatePlayerBehaviorEvent(model.PlayerID, $"修改備註:{myMemoText}");

        }

    }
}
