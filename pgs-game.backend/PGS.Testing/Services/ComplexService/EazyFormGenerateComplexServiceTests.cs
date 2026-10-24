using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EazyFormGenerateComplexServiceTests : BaseTest
    {
        public GeneralTagRepository GeneralTagRepository { get; set; }


        public IEazyFormGenerateComplexService ServiceInstance { get; set; }


        [TestMethod]
        public void GetForm_SaveForm_Test()
        {
            var query = GeneralTagRepository.GetPagingByFilter(new QueryGeneralTagRequestViewModel
            {
                PageSize = 10
            });

            if(query.Items.IsNotEmptyNull())
            {
                var matchFirst = query.Items
                    .OrderBy(x =>
                    {
                        if (x.Name.Contains("MSTest"))
                        {
                            return 1;
                        }

                        return 999;
                    })
                    .FirstOrDefault();

                int userID = _TEST_DATA.Default.UserID;
                var queryData = ServiceInstance.GetForm(userID, new LazyCommonRequestModel
                {
                    TargetID = matchFirst.TagID,
                    CategoryID = EazyFormTypeEnum.Edit_GeneralTag.ToInteger()
                });

                ServiceInstance.SaveForm(userID, queryData);
                return;
            }

            return;
        }

    }
}

