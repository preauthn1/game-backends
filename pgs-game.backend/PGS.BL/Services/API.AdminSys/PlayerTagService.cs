using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public class PlayerTagService : BaseAdminSysService, IPlayerTagService
    {

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }




        public CRUDResultModel Create(int userID, PlayerTagCreateRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            CheckViewPlayerPermission(userID, model.PlayerID);

            var queryExist = PlayerTagMappingRepository.GetPlayerTag(model.PlayerID, model.TagID);

            if (queryExist != null)
                return new CRUDResultModel();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var newEntity = model.ReturnInjecteObject<PlayerTagMappingEntity>();

            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.CreateBy = currentUser.UserName;

            PlayerTagMappingRepository.Create(newEntity);

            //更新cache
            CacheRemoveHelper.RemoveCache_WhenUpdatePlayerTag(model.PlayerID, model.TagID);

            return new CRUDResultModel
            {
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public void Delete(int userID, PlayerTagUpdateRequestModel model)
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            CheckViewPlayerPermission(userID, model.PlayerID);

            var queryExsit = PlayerTagMappingRepository.GetPlayerTag(model.PlayerID, model.TagID);

            if (queryExsit == null)
                return;

            PlayerTagMappingRepository.DeleteByModel(new { PlayerID = model.PlayerID, TagID = model.TagID });

            //更新cache
            CacheRemoveHelper.RemoveCache_WhenUpdatePlayerTag(model.PlayerID , model.TagID);

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.Player_RemoveTag, model.PlayerID, $"tagID: {model.TagID}");
            WriteOperateLog(log);

            return;
        }


    }

}
