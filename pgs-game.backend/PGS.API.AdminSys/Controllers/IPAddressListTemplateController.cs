using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using System.Linq;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Utility.Extensions;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Utility.Utility.ThirdParty;
using target.Utility.Utility;
using target.Models.ViewModels.Base;
using target.Models;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models.Paging;

namespace target.API.AdminSys.Controllers
{
    public class IPAddressListTemplateController : BaseController
    {
        public IIPAddressListTemplateService IPAddressListTemplateService { get; set; }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_Search)]
        [Route("ip-templates")]
        [HttpGet]
        public PagedList<IPAddressListTemplateViewModel> GetByPagingFilter([FromQuery]QueryIPAddressListTemplateApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = IPAddressListTemplateService.GetByPagingFilter(userID , filter);
            return query;
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_Search)]
        [Route("ip-templates/{id}")]
        [HttpGet]
        public IPAddressListTemplateViewModel GetById(int id)
        {
            int userID = GetCurrentUserID();

            var query = IPAddressListTemplateService.GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
            {
                IDs = id.ToListCollection()
            });

            return query.Items.FirstOrDefault();
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit)]
        [Route("ip-templates")]
        [HttpPost]
        public CRUDResultModel Create([FromBody] IPAddressListTemplateCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IPAddressListTemplateService.Create(userID, model);
            return query;
        }

        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit)]
        [Route("ip-templates")]
        [HttpPut]
        public CRUDResultModel Update([FromBody] IPAddressListTemplateUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IPAddressListTemplateService.Update(userID, model);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_Delete)]
        [Route("ip-templates/{templateID}")]
        [HttpDelete]
        public CRUDResultModel Delete(int templateID)
        {
            int userID = GetCurrentUserID();
            var query = IPAddressListTemplateService.Delete(userID, templateID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_AppendNewIP)]
        [Route("ip-templates/append-single-ip")]
        [HttpPost]
        public CRUDResultModel AppendNewIpToSetting([FromBody] IPAddressListAppendRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IPAddressListTemplateService.AppendNewIpToSetting(userID, model);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Merchant_IPAddressTemplate_AdvanceEdit)]
        [Route("ip-templates/assign-merchant")]
        [HttpPost]
        public CRUDResultModel UpdateMultipleAssignMerchant([FromBody] IPAddressListTemplateUpdateAssignMerchantIDRequestModel model)
        {
            int userID = GetCurrentUserID();

            if(model.MerchantIDs.IsNotEmptyNull())
            {
                var query = IPAddressListTemplateService.UpdateMultipleAssignMerchant(userID, model);
                return query;
            }
            else
            {
                var query = IPAddressListTemplateService.UpdateAssignMerchant(userID, model);
                return query;
            }
        }

    }
}
