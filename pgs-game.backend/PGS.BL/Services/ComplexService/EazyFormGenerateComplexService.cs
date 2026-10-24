using target.DA.Repository;
using target.Utility;
using target.Models.Table;
using System;
using System.Linq;
using target.BL.Interface;
using target.Utility.Helper;
using System.Collections.Generic;
using target.Utility.Extensions;
using example.invalid;
using target.Utility.Utility.ThirdParty;
using System.Threading;
using target.Models.Models;
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models;
using target.Models.CultureText;
using target.Models.ViewModels.GameVendor;
using example.invalid;

namespace target.BL.Services
{

    public partial class EazyFormGenerateComplexService : BaseAdminSysService, IEazyFormGenerateComplexService
    {
        public IGeneralTagService GeneralTagService { get; set; }


        /// <summary>
        /// 取得表單資訊 (Get)
        /// </summary>
        public EazyFormGenerateViewModel GetForm(int userID, LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.CategoryID).Rule_NotZeroNotMinus();

            EazyFormGenerateViewModel viewModel = null;

            EazyFormTypeEnum typeValue = (EazyFormTypeEnum)filter.CategoryID;

            switch (typeValue)
            {
                case EazyFormTypeEnum.Edit_GeneralTag:
                    viewModel = GetGeneralTag(filter);
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            viewModel.CategoryID = filter.CategoryID;
            return viewModel;
        }




        private EazyFormGenerateViewModel GetGeneralTag(LazyCommonRequestModel filter)
        {
            var query = GeneralTagService.GetByID((int)filter.TargetID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            return new EazyFormGenerateViewModel
            {
                TitleExtraInfo = $"({query.TagID})",
                MyModel = query,
                FormItems = new List<EazyFormItemViewModel>
                {
                    new EazyFormItemViewModel("name", "TableHeader.__displayName"),
                    new EazyFormItemViewModel("memo", "TableHeader.__memo")
                }
            };
        }


    }
}