using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IIPAddressListTemplateService
    {

        PagedList<IPAddressListTemplateViewModel> GetByPagingFilter(int userID, QueryIPAddressListTemplateApiRequestParameterModel filter);


        CRUDResultModel Create(int userID, IPAddressListTemplateCreateRequestModel model);


        CRUDResultModel Update(int userID, IPAddressListTemplateUpdateRequestModel model);


        CRUDResultModel UpdateMultipleAssignMerchant(int userID, IPAddressListTemplateUpdateAssignMerchantIDRequestModel model);


        WhiteListNewIPsCheckResultModel AnalyzeAnyNewIPs(int ipTemplateID, string appendIPs);


        CRUDResultModel AppendNewIpToSetting(int userID, IPAddressListAppendRequestModel model);


        CRUDResultModel AppendMultipleIPsToSetting(int userID, IPAddressListMultipleAppendRequestModel model);


        CRUDResultModel UpdateAssignMerchant(int userID, IPAddressListTemplateUpdateAssignMerchantIDRequestModel model);


        CRUDResultModel Delete(int userID, int targetID);


        void SetWhiteListIPsForSingleMerchant(int userID, int merchantID, List<string> newIPs);

    }
}
