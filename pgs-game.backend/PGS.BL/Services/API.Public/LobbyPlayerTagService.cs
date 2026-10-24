using System;
using System.Linq;
using System.Collections.Generic;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.BL.Services;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility.Helper.Dependency;
using target.Models.CultureText;
using target.Utility.Extensions.Web;
using Lck.Utility.Extensions.Helper;
using target.Models;

namespace target.BL.Interface
{

    public class LobbyPlayerTagService : BaseGameService, ILobbyPlayerTagService
    {

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }


        public void AddPlayerTagFromLobby(int playerID , LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            //ps. 這個function應該已經被棄用了
            List<int> availableTagIDs = new List<int>
            {
                SystemTagEnum.Player_SuspiciousWarning.ToInteger()
            };

            if(!availableTagIDs.Contains(model.TagID))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("TagID"));
            }

            PlayerTagMappingRepository.CreateTryCatch(new PlayerTagMappingEntity
            {
                CreateBy = "system",
                CreateTime = DateTimeHelper.GetCurrentTime(),
                PlayerID = playerID,
                TagID = model.TagID
            });

            //清除緩存
            CacheRemoveHelper.RemoveCache_WhenUpdatePlayerTag(playerID, model.TagID);
        }

    }
}
