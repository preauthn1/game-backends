using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IOrganizationService
    {
        PagedList<OrganizationViewModel> GetPagingByFilter(QueryOrganizationApiRequestParameterModel filter);

        OrganizationViewModel GetByID(int userID, int targetID);

        void Create(int userID , OrganizationCreateRequestModel model);

        void Update(int userID, OrganizationUpdateRequestModel model);

        List<OrganizationCustomizedSettingViewModel> GetCustomizedSettings(int userID , OrganizationCustomizedSettingQueryModel model);

        void CreateOrUpdateCustomizedSetting(int userID, OrganizationCustomizedSettingViewModel model);

        void Delete(int targetID);

    }
}
