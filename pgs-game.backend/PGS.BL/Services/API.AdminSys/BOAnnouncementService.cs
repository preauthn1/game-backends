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
    public class BOAnnouncementService : BaseAdminSysService, IBOAnnouncementService
    {

        public BOAnnouncementRepository BOAnnouncementRepository { get; set; }

        public BOAnnouncementReadRepository BOAnnouncementReadRepository { get; set; }

        public AssignTemplateParticipaterRepository AssignTemplateParticipaterRepository { get; set; }


        public PagedList<BOAnnouncementViewModel> GetByPagingFilter(int userID, QueryBOAnnouncementApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                //功能先不做太複雜。只能看到&編輯 自己發的文
                filter.OwnerUserID = currentUser.UserID;
            }

            var query = BOAnnouncementRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<BOAnnouncementEntity, BOAnnouncementViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    return convert;
                })
                .ToList();

            return viewModels;
        }



        public BOAnnouncementViewModel GetById(int userID, int id)
        {
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            var query = BOAnnouncementRepository.GetById(id);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var convert = ConvertToViewModel(query);

            var assignSetting = AssignTemplateParticipaterRepository.GetByFilter(new QueryAssignTemplateParticipaterRequestViewModel
            {
                AssignTemplateID = query.AssignTemplateID,
            });

            var matchFirst = assignSetting.FirstOrDefault();

            convert.AssignParticipate = new AssignParticipateRequest
            {
                AssignType = matchFirst.AssignType,
                ParticipaterIDs = assignSetting.Select(x => x.ParticipaterID).ToList()
            };

            return convert;
        }


        public CRUDResultModel Create(int userID, BOAnnouncementCreateRequestModel model)
        {
            //type目前沒啥意義 先不用勾選
            model.Type = BOAnnouncementTypeEnum.Common.ToInteger();

            ValidateInput(model);

            model.RuleFor(x => x.AssignParticipate).Rule_ModelNotNull();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (string.IsNullOrWhiteSpace(model.ReplaceCreateBy))
            {
                model.ReplaceCreateBy = currentUser.UserName;
            }

            ValidateAssignTypeAndDefaultValue(model.AssignParticipate);

            var newEntity = model.ReturnInjecteObject<BOAnnouncementEntity>();

            Guid newUUID = Guid.NewGuid();

            var now = DateTimeHelper.GetCurrentTime(); ;
            newEntity.Content = ReplaceContent(newEntity.Content);
            newEntity.Status = NormalActiveStatus.Enable.ToByte();
            newEntity.CreateTime = now;
            newEntity.CreateBy = model.ReplaceCreateBy;
            newEntity.UpdateTime = now;
            newEntity.UpdateBy = currentUser.UserName;
            newEntity.AssignTemplateID = newUUID;
            newEntity.AssignType = model.AssignParticipate.AssignType;
            newEntity.OwnerUserID = currentUser.UserID;
            newEntity.OwnerOrganizationID = currentUser.OrganizationID;

            var resultId = BOAnnouncementRepository.Create(newEntity);

            var insertItems = new List<AssignTemplateParticipaterEntity>();

            foreach (var participaterID in model.AssignParticipate.ParticipaterIDs)
            {
                insertItems.Add(new AssignTemplateParticipaterEntity
                {
                    AssignTemplateID = newUUID,
                    AssignType = model.AssignParticipate.AssignType,
                    ParticipaterID = participaterID
                });
            }

            AssignTemplateParticipaterRepository.CreateMany(insertItems);

            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        private string ReplaceContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            //請勿刪除這個取代
            return content.Replace("</p><p>", " </p><p> ");
        }


        public CRUDResultModel Update(int userID, BOAnnouncementUpdateRequestModel model)
        {
            model.RuleFor(x => x.BOAnnouncementID).Rule_NotZeroNotMinus();

            ValidateInput(model);

            model.RuleFor(x => x.AssignParticipate).Rule_ModelNotNull();

            ValidateAssignTypeAndDefaultValue(model.AssignParticipate);

            var queryExsit = BOAnnouncementRepository.GetById(model.BOAnnouncementID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            queryExsit.Status = model.Status;
            queryExsit.Title = model.Title;
            queryExsit.ImageUrl = model.ImageUrl;
            queryExsit.Content = ReplaceContent(model.Content);
            queryExsit.Type = model.Type;
            queryExsit.HintType = model.HintType;
            queryExsit.BeginTime = model.BeginTime;
            queryExsit.EndTime = model.EndTime;
            queryExsit.HintType = model.HintType;
            queryExsit.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryExsit.UpdateBy = currentUser.UserName;
            queryExsit.AssignType = model.AssignParticipate.AssignType;

            BOAnnouncementRepository.Update(queryExsit);

            var insertItems = new List<AssignTemplateParticipaterEntity>();

            foreach (var participaterID in model.AssignParticipate.ParticipaterIDs)
            {
                insertItems.Add(new AssignTemplateParticipaterEntity
                {
                    AssignTemplateID = queryExsit.AssignTemplateID,
                    AssignType = model.AssignParticipate.AssignType,
                    ParticipaterID = participaterID
                });
            }

            AssignTemplateParticipaterRepository.DeleteByModel(new { AssignTemplateID = queryExsit.AssignTemplateID });
            AssignTemplateParticipaterRepository.CreateMany(insertItems);

            return new CRUDResultModel(true);
        }


        public CRUDResultModel Delete(int userID, int targetID)
        {
            var queryExsit = BOAnnouncementRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExsit.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            queryExsit.UpdateTime = DateTimeHelper.GetCurrentTime();

            BOAnnouncementRepository.Update(queryExsit);

            return new CRUDResultModel(true);
        }


        public BOAnnouncementHomePageViewModel GetVisibleAnnouncements(int userID)
        {
            var viewModelResult = new BOAnnouncementHomePageViewModel();
            viewModelResult.Items = new List<HomePageBOAnnouncementItemViewModel>();

            var queryAnn = BOAnnouncementRepository.GetPagingByFilter(new QueryBOAnnouncementRequestViewModel
                {
                    IsOnProgressing = true,
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    PageSize = 20,
                })
                .Items;

            if (queryAnn.IsEmptyOrNull())
            {
                return viewModelResult;
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            //如果屬於全平台可查看的公告 就不用特地另外查DB了
            var assignTemplateIDs_allVisible = queryAnn.Where(x=> x.AssignType == (int)AssignTypeEnum.All).Select(x => x.AssignTemplateID).ToList();
            var assignTemplateIDs_other = queryAnn.Where(x => x.AssignType != (int)AssignTypeEnum.All).Select(x => x.AssignTemplateID).ToList();

            List<Guid> visibleTemplateIDs = new List<Guid>();

            visibleTemplateIDs.AddRange(assignTemplateIDs_allVisible);

            if (assignTemplateIDs_other.IsNotEmptyNull())
            {
                var queryAssign = AssignTemplateParticipaterRepository.GetByFilter(new QueryAssignTemplateParticipaterRequestViewModel
                {
                    AssignTemplateIDs = assignTemplateIDs_other
                });

                foreach (var templateID in assignTemplateIDs_other)
                {
                    var assignParticipates = queryAssign.Where(x => x.AssignTemplateID == templateID).ToList();

                    var matchFisrt = assignParticipates.FirstOrDefault();

                    if (matchFisrt == null)
                        continue;

                    switch ((AssignTypeEnum)matchFisrt.AssignType)
                    {
                        case AssignTypeEnum.All:
                            {
                                visibleTemplateIDs.Add(templateID);
                                break;
                            }

                        case AssignTypeEnum.SpecificOrganizationCanView:
                            {
                                if (assignParticipates.Any(z => z.ParticipaterID == currentUser.OrganizationID))
                                {
                                    visibleTemplateIDs.Add(templateID);
                                }
                                break;
                            }

                        case AssignTypeEnum.SpecificReferenceMerchantCanView:
                            {
                                if (assignParticipates.Any(z => currentUser.AvailableMerchantIDs.Contains(z.ParticipaterID)))
                                {
                                    visibleTemplateIDs.Add(templateID);
                                }
                                break;
                            }

                        case AssignTypeEnum.SpecificUserCanView:
                            {
                                if (assignParticipates.Any(z => z.ParticipaterID == currentUser.UserID))
                                {
                                    visibleTemplateIDs.Add(templateID);
                                }
                                break;
                            }

                        default:
                            break;
                    }
                }
            }

            var availableAnn = queryAnn.Where(x => visibleTemplateIDs.Contains(x.AssignTemplateID)).ToList();

            if (availableAnn.IsEmptyOrNull())
                return viewModelResult;

            var queryRead = BOAnnouncementReadRepository.GetByFilter(new QueryBOAnnouncementReadRequestViewModel
            {
                BOAnnouncementIDs = availableAnn.Select(x => x.BOAnnouncementID).ToList()
            });

            viewModelResult.Items = availableAnn.Select(z =>
                {
                    var convert1 = ConvertToViewModel(z);
                    var convert2 = convert1.ReturnInjecteObject<HomePageBOAnnouncementItemViewModel>();
                    convert2.IsRead = queryRead.Any(read => read.BOAnnouncementID == convert2.BOAnnouncementID);
                    convert2.ContentStripHtml = convert2.Content.StripHTML();
                    return convert2;
                })
                .ToList();

            viewModelResult.PopupAnns = viewModelResult
                .Items
                .Where(x => x.HintType == BOAnnouncementHintTypeEnum.Popup.ToInteger())
                .Where(x=> !x.IsRead)
                .ToList();

            return viewModelResult;
        }


        public void SetAnnouncementToRead(int userID , int announcementID)
        {
            BOAnnouncementReadRepository.CreateTryCatch(new BOAnnouncementReadEntity
            {
                BOAnnouncementID = announcementID,
                UserID = userID,
                CreateTime = DateTimeHelper.GetCurrentTime()
            });
        }


        private BOAnnouncementViewModel ConvertToViewModel(BOAnnouncementEntity query)
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<BOAnnouncementViewModel>();
            convert.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((BOAnnouncementTypeEnum)query.Type);
            convert.HintTypeText = MultiLanguageTextHelper.GetEnumsCultureText((BOAnnouncementHintTypeEnum)query.HintType);
            convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)query.Status);
            convert.AssignTypeText = MultiLanguageTextHelper.GetEnumsCultureText((AssignTypeEnum)query.AssignType);
            return convert;
        }


        private void ValidateInput<T>(T model) where T : class, IBOAnnouncementRequestModel
        {
            model.RuleFor(x => x.Title).Rule_NotEmptyText().Rule_MaxLengthOK(50);
            model.RuleFor(x => x.Content).Rule_MaxLengthOK(2000);
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.HintType).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BeginTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();
        }


        private void ValidateAssignTypeAndDefaultValue(AssignParticipateRequest assignSetting)
        {
            assignSetting.RuleFor(x => x.AssignType).Rule_NotZeroNotMinus();

            if (assignSetting.AssignType == (int)AssignTypeEnum.SpecificOrganizationCanView ||
                assignSetting.AssignType == (int)AssignTypeEnum.SpecificUserCanView ||
                assignSetting.AssignType == (int)AssignTypeEnum.SpecificReferenceMerchantCanView)
            {
                assignSetting.RuleFor(x => x.ParticipaterIDs).Rule_IsNotEmptyCollection();
            }

            if (assignSetting.AssignType == (int)AssignTypeEnum.All)
            {
                assignSetting.ParticipaterIDs = new List<int>();
                assignSetting.ParticipaterIDs.Add(0);
            }
        }


    }

}
