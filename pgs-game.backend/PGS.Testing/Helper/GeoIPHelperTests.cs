//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using target.BL.Interface;
//using target.DA.Repository;
//using target.Models.CoreLibs;
//using target.Models.Enums;
//using target.Models.Models;
//using target.Models.ViewModels;
//using target.Utility.Extensions;
//using target.Utility.Helper;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace target.Testing
//{
//    [TestClass]
//    public class GeoIPHelperTests : BaseTest
//    {

//        [TestMethod]
//        public void Analyze_IPAddress_Country_Group_Test()
//        {
//            if (IsNotDeveloperComputerName())
//                return;

//            // Step 1. 使用此SQL 查出某個商戶的客戶IP
//            /*
//                SELECT distinct IPAddress + ',' FROM[PGS_Transaction].[dbo].[VendorSessionToken] WITH(NOLOCK)
//                WHERE CreateTime >= '2024-02-01' AND CreateTime< '2024-02-02' AND PlayerID in
//                (
//                    SELECT PlayerID FROM[PGS_Master].[dbo].[Player] WITH(NOLOCK)
//                    WHERE MerchantID = 175080
//                 )
//            */

//            // Step 2. 貼到這邊

//            string rawText = @"
//0.0.0.0,
//0.0.0.0
//";

//            var myIPs = rawText.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();

//            var country = new List<string>();

//            foreach(var ip in myIPs)
//            {
//                if (string.IsNullOrWhiteSpace(ip))
//                    continue;

//                var result1 = GeoIPHelper.GetIPAddressInfo(ip.Trim());
//                country.Add(result1.ISOCode);
//            }

//            //分析各個地區的分布
//            var groupByResult = country.GroupBy(x => x)
//                .Select(x => new
//                {
//                    Country = x.Key,
//                    TotalCount = x.Count()
//                })
//                .OrderByDescending(z=> z.TotalCount)
//                .ToList();

//            return;
//        }



//        [TestMethod]
//        public void GetIPAddressInfo_Test()
//        {
//            var result1 = GeoIPHelper.GetIPAddressInfo("0.0.0.0");
//            Assert.IsTrue(result1.ISOCode == "TW");
//            return;
//        }




//    }
//}

