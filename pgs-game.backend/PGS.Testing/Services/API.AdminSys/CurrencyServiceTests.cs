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
    public class CurrencyServiceTests : BaseTest
    {

        public ICurrencyService ServiceInstance { get; set; }


        [TestMethod]
        public void GetAllCurrencyList_Test()
        {
            var query = ServiceInstance.GetAllCurrencyList(_TEST_DATA.Default.UserID);
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void UpdateRate_Test()
        {
            List<decimal> lisfOfRates_USDT = new List<decimal>
            {
                0.2114M,
                0.2115M,
                0.2116M,
            };

            List<decimal> lisfOfRates_TWD = new List<decimal>
            {
                6.95M,
                6.96M,
                6.97M,
            };

            decimal rateUSDT = lisfOfRates_USDT.OrderByRandom().FirstOrDefault();

            decimal rateTWD = lisfOfRates_TWD.OrderByRandom().FirstOrDefault();

            var resultUSDT = ServiceInstance.Update(_TEST_DATA.Default.UserID , new CurrencyUdpateRequestModel
            {
                Currency = "MYR",
                ExchangeCurrency = "USDT",
                ExchangeRate = rateUSDT
            });

            var resultTWD = ServiceInstance.Update(_TEST_DATA.Default.UserID, new CurrencyUdpateRequestModel
            {
                Currency = "MYR",
                ExchangeCurrency = "TWD",
                ExchangeRate = rateTWD
            });

            //ResultNumber 返回的是更新前的數值
            decimal diffUSDT = (resultUSDT.ResultNumber / rateUSDT).ToAbs() - 1;
            decimal diffTWD = (resultTWD.ResultNumber / rateTWD).ToAbs() - 1;

            //差異小於9%
            Assert.IsTrue(diffUSDT.ToAbs() < 0.09M);
            Assert.IsTrue(diffTWD.ToAbs() < 0.09M);
            return;
        }



        [TestMethod]
        public void UpdateRate_ParameterError_Test()
        {
            AssertExpectExceptionProcess(() =>
            {
                ServiceInstance.Update(_TEST_DATA.Default.UserID, new CurrencyUdpateRequestModel
                {
                    Currency = "MYR",
                    ExchangeCurrency = "",
                    ExchangeRate = 123
                });

            }, ApiErrorCode.InvalidParameter_TextEmpty);

            return;
        }


    }
}

