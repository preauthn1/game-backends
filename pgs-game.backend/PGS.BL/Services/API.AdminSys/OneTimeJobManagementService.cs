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
    public class OneTimeJobManagementService : BaseAdminSysService, IOneTimeJobManagementService
    {

        public OneTimesJobSettingRepository OneTimesJobSettingRepository { get; set; }


        public PagedList<OneTimeJobViewModel> GetByPagingFilter(int userID, QueryOneTimeJobApiRequestParameterModel filter)
        {
            var cureentUser = GetCurrentOwnPermissionByCache(userID);

            filter.Types = filter.Type.ToIntegerListCollection();

            var query = OneTimesJobSettingRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<OneTimesJobSettingEntity, OneTimeJobViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<OneTimeJobViewModel>(); ;
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public CRUDResultModel Create(int userID, OneTimeJobCreateRequestModel model)
        {
            model.RuleFor(x => x.Type).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Memo).Rule_NotEmptyText();
            model.RuleFor(x => x.NextTime).Rule_NotEmptyDateTime();

            var newEntity = model.ReturnInjecteObject<OneTimesJobSettingEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);
            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.CreateBy = currentUser.UserName;

            var resultId = OneTimesJobSettingRepository.Create(newEntity);

            return new CRUDResultModel
            {
                ResultLongId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        public CRUDResultModel Delete(int userID, long targetID)
        {
            var queryExsit = OneTimesJobSettingRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            OneTimesJobSettingRepository.Delete(targetID);
            return new CRUDResultModel(true);
        }


        private void SetExtraProperty(List<OneTimeJobViewModel> query)
        {
            if (query.IsEmptyOrNull())
                return;

            foreach (var item in query)
            {
                item.TypeText = MultiLanguageTextHelper.GetEnumsCultureText((OneTimesJobTypeEnum)item.Type);
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((OneTimesJobStatusEnum)item.Status);
            }
        }


    }

}
