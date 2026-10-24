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
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class RTPChangeLogService : BaseAdminSysService, IRTPChangeLogService
    {

        public GameVendorRTPChangeRecordRepository GameVendorRTPChangeRecordRepository { get; set; }


        public PagedList<RTPChangeLogViewModel> GetByPagingFilter(int userID, QueryGameVendorRTPChangeRecordRequestParameterModel filter)
        {
            var query = GameVendorRTPChangeRecordRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<GameVendorRTPChangeRecordEntity, RTPChangeLogViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            return viewModels;
        }


        private RTPChangeLogViewModel ConvertToViewModels(GameVendorRTPChangeRecordEntity query)
        {
            var convert = query.ReturnInjecteObject<RTPChangeLogViewModel>();
            return convert;
        }


    }

}
