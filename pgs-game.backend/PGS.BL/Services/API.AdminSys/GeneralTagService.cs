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
    public class GeneralTagService : BaseAdminSysService, IGeneralTagService
    {

        public GeneralTagRepository GeneralTagRepository { get; set; }

        public GeneralTagMappingRepository GeneralTagMappingRepository { get; set; }


        public PagedList<GeneralTagViewModel> GetByPagingFilter(int userID, QueryGeneralTagApiRequestParameterModel filter)
        {
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            var query = GeneralTagRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<GeneralTagEntity, GeneralTagViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<GeneralTagViewModel>(); ;
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }



        public GeneralTagViewModel GetByID(int tagID)
        {
            var query = GeneralTagRepository.GetById(tagID);

            var viewModel = ConvertToViewModel(query);

            var queryMapping = GeneralTagMappingRepository.GetByTagID(tagID);

            viewModel.AssignTargetIDs = GetAssignerIDs(queryMapping);

            return viewModel;
        }


        public CRUDResultModel Create(int userID, GeneralTagCreateRequestModel model)
        {
            model.RuleFor(x => x.TagID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Name).Rule_NotEmptyText();

            model.RuleFor(x => x.TagID).Rule_MinMaxValue(model.Type, model.Type + 9999);

            var queryExist = GeneralTagRepository.GetById(model.TagID);

            if (queryExist != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);

            var newEntity = model.ReturnInjecteObject<GeneralTagEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);
            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();

            var resultId = GeneralTagRepository.Create(newEntity);

            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Update(int userID, GeneralTagUpdateBasicInfoRequestModel model)
        {
            model.RuleFor(x => x.TagID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Name).Rule_NotEmptyText();

            var queryExist = GeneralTagRepository.GetById(model.TagID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            queryExist.Name = model.Name;
            queryExist.Memo = model.Memo;

            GeneralTagRepository.Update(queryExist);

            return new CRUDResultModel();
        }


        public CRUDResultModel UpdateAssigner(int userID, GeneralTagUpdateRequestModel model)
        {
            model.RuleFor(x => x.TagID).Rule_NotZero().Rule_NotMinus();

            var queryExist = GeneralTagRepository.GetByIdOrThrowException(model.TagID);

            model.AssignTargetIDs = model.AssignTargetIDs ?? new List<int>();

            var insertItems = new List<GeneralTagMappingEntity>();

            var now = DateTimeHelper.GetCurrentTime();

            foreach (int targetID in model.AssignTargetIDs)
            {
                var myInstance = new GeneralTagMappingEntity
                {
                    CreateTime = now,
                    TagID = model.TagID,
                };

                myInstance.TargetID = targetID;

                insertItems.Add(myInstance);
            }

            // Step 1. 清除原資料
            GeneralTagMappingRepository.DeleteByModel(new
            {
                TagID = model.TagID
            });

            // Step 2. 批次寫入
            GeneralTagMappingRepository.BulkInsert(insertItems);

            // Step 3. 寫log
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateTagAssigner, model.TagID, $"{model.ConvertToJsonString()}");
            WriteOperateLog(log);


            return new CRUDResultModel(true);
        }


        public CRUDResultModel AppendteTagsToAssigner(int userID, GeneralTagUpdateByAssignerRequestModel model)
        {
            model.RuleFor(x => x.AssignTargetID).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.Type).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.TagIDs).Rule_NotEmptyNull();

            foreach (var tagID in model.TagIDs)
            {
                int minValue = model.Type;
                int maxValue = model.Type + 9999;

                var param = new
                {
                    tagID = model.Type
                };

                param.RuleFor(x => x.tagID).Rule_MinMaxValue(minValue, maxValue);
            }

            var queryExist = GeneralTagMappingRepository.GetByFilter(new QueryGeneralTagMappingRequestViewModel
            {
                TargetIDs = model.AssignTargetID.ToListCollection(),
                TagIDs = model.TagIDs,
            });

            foreach(var tagID in model.TagIDs)
            {
                bool isExist = queryExist.Any(x => x.TagID == tagID && x.TargetID == model.AssignTargetID);

                if(!isExist)
                {
                    GeneralTagMappingRepository.Create(new GeneralTagMappingEntity
                    {
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        TagID = tagID,
                        TargetID = model.AssignTargetID,
                        ExtraSettingValue = ""
                    });
                }
            }

            return new CRUDResultModel();
        }



        public CRUDResultModel Delete(int userID, int targetID)
        {
            var queryExsit = GeneralTagRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            GeneralTagRepository.Delete(targetID);
            return new CRUDResultModel(true);
        }


        private GeneralTagViewModel ConvertToViewModel(GeneralTagEntity query)
        {
            //將model轉成viewModels
            var convert = query.ReturnInjecteObject<GeneralTagViewModel>();
            return convert;
        }


        private List<int> GetAssignerIDs(List<GeneralTagMappingEntity> query)
        {
            if (query.IsEmptyOrNull())
                return new List<int>();

            return query.Select(z => z.TargetID).ToList();
        }


        private void SetExtraProperty(List<GeneralTagViewModel> query)
        {
            if (query.IsEmptyOrNull())
                return;

            var queryMappingCount = GeneralTagMappingRepository.GetEachTagMappingCount();

            foreach (var item in query)
            {
                item.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((TagTypeEnum)item.Type);
                item.AssignTargetCount = queryMappingCount.FirstOrDefault(x => x.OwnerID == item.TagID)?.TotalCount ?? 0;
            }
        }


    }

}
