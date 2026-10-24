using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web.Mvc;
using DataTables.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Pay;
using game.Entity.PerLog;
using game.Entity.Users;
using game.Entity.ViewModels.Query;
using game.Infrastructure;
using game.Service;
using game.Service.DAO;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity.RawEntity;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-標記與備註
    /// </summary>
    public class UserTagController : BaseController
    {

        public UserMemoRepository UserMemoRepository { get; set; }

        public UserTagMappingRepository UserTagMappingRepository { get; set; }

        public TagsRepository TagsRepository { get; set; }



        [NoAuthorization]
        public ActionResult GetUserTags(int userID)
        {
            var query = UserTagMappingRepository.GetByFilter(new QueryUserTagMappingRequestViewModel
            {
                UserIDs = userID.ToListCollection()
            });
            List<UserTagViewModel> convert = ConvertToViewModels(query);

            return Json(convert, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult GetUserMemos(int userID)
        {
            var query = UserMemoRepository.GetByUserId(userID);

            var convert = query
                .OrderByDescending(x => x.CreateTime)
                .Select(x => new UserMemoViewModel
                {
                    Id = x.Id,
                    Memo = x.Memo,
                    CreateBy = x.CreateBy,
                    CreateDate = x.CreateTime.ToString("yyyy-MM-dd"),
                    CreateTime = x.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                });

            return Json(convert, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [NoAuthorization]
        public ActionResult AddUserTag(UserTagUpdateRequestModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TagID).Rule_NotZeroNotMinus();

            string userName = GetCurrentUserName();


            var queryExist = UserTagMappingRepository.GetByFilter(new QueryUserTagMappingRequestViewModel
            {
                UserIDs = model.UserID.ToListCollection(),
                TagIDs = model.TagID.ToListCollection()
            });

            if (queryExist.IsNotEmptyNull())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            UserTagMappingRepository.Create(new UserTagMappingRawEntity
            {
                UserID = model.UserID,
                TagID = model.TagID,
                CreateBy = userName,
                CreateTime = DateTimeHelper.GetCurrentTime(),
            });

            var result = new CRUDResultModel
            {
                IsSuccess = true,
                Message = SystemConst.SUCCESS_MSG
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [NoAuthorization]
        public ActionResult AddUserMemo(UserMemoUpdateRequestModel model)
        {
            model.RuleFor(x => x.UserID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Memo).Rule_NotEmptyText();

            string userName = GetCurrentUserName();

            UserMemoRepository.Create(new Entity.RawEntity.UserMemoRawEntity
            {
                Memo = model.Memo,
                CreateBy = userName,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                UserID = model.UserID
            });

            var result = new CRUDResultModel
            {
                IsSuccess = true,
                Message = SystemConst.SUCCESS_MSG
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [NoAuthorization]
        public ActionResult DeleteUserMemo(UserMemoDeleteRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZeroNotMinus();

            string userName = GetCurrentUserName();

            UserMemoRepository.Delete(model.Id);

            var result = new CRUDResultModel
            {
                IsSuccess = true,
                Message = SystemConst.SUCCESS_MSG
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [NoAuthorization]
        public ActionResult DeleteUserTag(UserMemoDeleteRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZeroNotMinus();

            UserTagMappingRepository.Delete(model.Id);

            var result = new CRUDResultModel
            {
                IsSuccess = true,
                Message = SystemConst.SUCCESS_MSG
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        private List<UserTagViewModel> ConvertToViewModels(List<UserTagMappingRawEntity> query)
        {
            if (query.IsEmptyOrNull())
                return new List<UserTagViewModel>();

            var queryAllTags = TagsRepository.GetAll();

            var convert = query
                .OrderByDescending(x=> x.CreateTime)
                .Select(x =>
                {
                    var matchTag = queryAllTags.FirstOrDefault(z => z.TagID == x.TagID);
                    var convertModel = new UserTagViewModel
                    {
                        TagID = x.TagID,
                        CreateBy = x.CreateBy,
                        CreateDate = x.CreateTime.ToString("yyyy-MM-dd"),
                        CreateTime = x.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        IsHighlightDisplay = matchTag?.UniqueKey == (int)TagUniqueKeyEnum.InternalTestAccount,
                        Id = x.Id,
                        TagName = matchTag?.TagName
                    };

                    return convertModel;
                })
                .ToList();

            return convert;
        }

    }


}