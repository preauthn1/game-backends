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

    public class AnnouncementManagementService : BaseAdminSysService, IAnnouncementManagementService
    {

        public AnnouncementRepository AnnouncementRepository { get; set; }

        public AnnouncementReadRepository AnnouncementReadRepository { get; set; }

        public ContentBlockRepository ContentBlockRepository { get; set; }


        public PagedList<AnnouncementPagingViewModel> GetByPagingFilter(int userID, QueryAnnouncementApiParameterModel filter)
        {
            filter.AssignMerchantIDs = filter.MerchantID.ToIntegerListCollection();

            var query = AnnouncementRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<AnnouncementEntity, AnnouncementPagingViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<AnnouncementPagingViewModel>();
                    convert.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((AnnouncementTypeEnum)x.Type);

                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }

        public AnnouncementDetailViewModel GetDetail(int userID, int announcementID)
        {
            var queryAnn = AnnouncementRepository.GetById(announcementID);

            if (queryAnn == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryAnnContent = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuid = queryAnn.AnnouncementGuid,
            });

            var viewModel = queryAnn.ReturnInjecteObject<AnnouncementDetailViewModel>();
            viewModel.BeginTimeTS_1000 = viewModel.BeginTime.ConvertToUnixTimestamp() * 1000;
            viewModel.EndTimeTS_1000 = viewModel.EndTime.ConvertToUnixTimestamp() * 1000;
            viewModel.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((AnnouncementTypeEnum)queryAnn.Type);


            bool isDisableEditTitle = false;
            bool isDisableEditDescriptiion = false;

            if(queryAnn.Type == (int)AnnouncementTypeEnum.LobbyAdvertisement)
            {
                isDisableEditDescriptiion = true;
                isDisableEditTitle = true;
            }

            viewModel.Contents = queryAnnContent.Select(z =>
                {
                    var convert = z.ReturnInjecteObject<AnnouncementContentBlockItemModel>();
                    convert.LanguageText = MultiLanguageTextHelper.GetLanguageCodeDisplayText(z.LanguageCode);
                    convert.IsDisableEditDescriptiion = isDisableEditDescriptiion;
                    convert.IsDisableEditTitle = isDisableEditTitle;
                    return convert;
                })
                .OrderBy(x=> x.LanguageCode == LanguageEnum.en_us.ToDashLangCode() ? 1: 999)
                .ThenBy(z=> z.Title.IsNotEmptyTextNotNull() ? 1 : 999)
                .ToList();

            return viewModel;
        }


        public void UpdateDetail(int userID, AnnouncementDetailViewModel model)
        {
            model.RuleFor(z => z.AnnouncementID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.Memo).Rule_NotEmptyText();
            model.RuleFor(z => z.BeginTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);

            var queryAnn = AnnouncementRepository.GetById(model.AnnouncementID);

            if (queryAnn == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            DateTime now = DateTimeHelper.GetCurrentTime();

            queryAnn.Memo = model.Memo;
            queryAnn.Status = model.Status;
            queryAnn.BeginTime = model.BeginTime;
            queryAnn.EndTime = model.EndTime;
            queryAnn.UpdateTime = now;
            queryAnn.UpdateBy = currentUser.UserName;
            queryAnn.MerchantSelectTemplateID = model.MerchantSelectTemplateID.ParseZeroToNull();

            AnnouncementRepository.Update(queryAnn);

            var queryAnnContent = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuid = queryAnn.AnnouncementGuid,
            });

            foreach (var entity in queryAnnContent)
            {
                var matchEditItem = model.Contents.FirstOrDefault(x => x.ContentBlockID == entity.ContentBlockID);

                entity.UpdateTime = now;
                entity.UpdateBy = currentUser.UserName;
                entity.Title = matchEditItem.Title;
                entity.Description = matchEditItem.Description;
                entity.ImageUrlMiddle = matchEditItem.ImageUrlMiddle;
                entity.LinkType = matchEditItem.LinkType;
                entity.LinkReference = matchEditItem.LinkReference;

                ContentBlockRepository.Update(entity);
            }

            return;
        }


        public CRUDResultModel QuickCreateEmptyAnnoucement(int userID, AnnouncementCreateRequestModel model)
        {
            model.RuleFor(z => z.Memo).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();
            DateTime today_tw_0000 = now.Date.AddHours(-8);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            Guid eventGuid = Guid.NewGuid();

            var type = AnnouncementTypeEnum.PopupAdvertisement.ToInteger();

            if (model.Type.HasValue)
                type = model.Type.Value;

            var newID = AnnouncementRepository.Create(new AnnouncementEntity
            {
                AnnouncementGuid = eventGuid,
                Status = NormalActiveStatus.Disable.ToInteger(),
                BeginTime = today_tw_0000,
                EndTime = today_tw_0000.AddDays(1),
                Type = type,
                CreateTime = now,
                UpdateTime = now,
                Memo = model.Memo,
                UpdateBy = currentUser.UserName
            });

            List<LanguageEnum> langs = new List<LanguageEnum>
            {
                LanguageEnum.en_us,
                LanguageEnum.zh_cn,
                LanguageEnum.ms_my,
                LanguageEnum.id_id,
                LanguageEnum.th_th
            };

            foreach (var lang in langs)
            {
                ContentBlockRepository.Create(new ContentBlockEntity
                {
                    OwnerGuid = eventGuid,
                    LanguageCode = lang.ToDashLangCode(),
                    LinkType = LinkTypeEnum.None.ToInteger(),
                    IsDefault = lang == LanguageEnum.en_us,
                    Title = "",
                    Description = "",
                    Memo = "",
                    CreateTime = now,
                    UpdateTime = now,
                    CreateBy = currentUser.UserName,
                    UpdateBy = currentUser.UserName,
                });
            }

            return new CRUDResultModel
            {
                ResultId = newID
            };
        }

        public void ClearAllRead(int userID, int announcementID)
        {
            var queryAnn = AnnouncementRepository.GetById(announcementID);

            if (queryAnn == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            AnnouncementReadRepository.DeleteByModel(new
            {
                AnnouncementID = announcementID
            });
        }


        public void Delete(int userID, int announcementID)
        {
            var queryAnn = AnnouncementRepository.GetById(announcementID);

            if (queryAnn == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryAnn.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            AnnouncementRepository.Update(queryAnn);
        }


        private void SetExtraProperty(List<AnnouncementPagingViewModel> items)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            foreach (var item in items)
            {
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)item.Status);
                item.IsExpired = now > item.EndTime;
                item.IsInProgress = now >= item.BeginTime && now < item.EndTime;
            }
        }


    }
}
