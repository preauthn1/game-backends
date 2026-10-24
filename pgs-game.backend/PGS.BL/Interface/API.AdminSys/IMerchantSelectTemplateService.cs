using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IMerchantSelectTemplateService
    {
        MerchantSelectTemplateViewModel GetByTemplateID(int userID, int templateID);

        CRUDResultModel CreateTemplate(int userID, MerchantSelectTemplateRequestModel model);

        void ResetCacheByTemplateID(int templateID);

        void RegenerateAssignMerchantIDsForAgentAssignMode(List<int> affectMerchantIDs);

        void Delete(int userID, int templateID);

        CRUDResultModel UpdateTemplate(int userID, MerchantSelectTemplateRequestModel model);
    }
}
