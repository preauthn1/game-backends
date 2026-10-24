using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class EazyTokenValidateServiceTests : BaseTest
    {

        public IEazyTokenValidateService ServiceInstance { get; set; }


        public EazyTokenLogRepository EazyTokenLogRepository { get; set; }


        [TestMethod]
        public void CheckIsValidToken_Test()
        {
            string limitValue = Guid.NewGuid().ToString();
            string myToken = [REDACTED];

            EazyTokenLogRepository.Create(new Models.Table.EazyTokenLogEntity
            {
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ExpiredTime = DateTimeHelper.GetCurrentTime().AddHours(SystemConstant.ReplayHistoryTokenExpiredHour),
                ReferenceValue = limitValue,
                Token = [REDACTED],
                Type = 0
            });

            ServiceInstance.CheckIsValidToken(myToken , new EazyTokenValidateRule
            {
                ValidateValueMatch = limitValue,
                CheckExpiredHourBeforeQueryDB = SystemConstant.ReplayHistoryTokenExpiredHour
            });

            return;
        }

    }
}

