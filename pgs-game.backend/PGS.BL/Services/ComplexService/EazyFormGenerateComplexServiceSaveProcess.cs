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
      
        /// <summary>
        /// 儲存submit
        /// </summary>
        public void SaveForm(int userID , EazyFormGenerateViewModel model)
        {
            model.RuleFor(x => x.CategoryID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.MyModel).Rule_ModelNotNull();

            EazyFormTypeEnum typeValue = (EazyFormTypeEnum)model.CategoryID;

            switch (typeValue)
            {
                case EazyFormTypeEnum.Edit_GeneralTag:
                    SaveGeneralTag(userID , model);
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


        private void SaveGeneralTag(int userID, EazyFormGenerateViewModel model)
        {
            var updateModel = model.MyModel.ConvertToJsonString().JsonConvertToModel<GeneralTagUpdateBasicInfoRequestModel>();
            GeneralTagService.Update(userID, updateModel);
        }


    }
}