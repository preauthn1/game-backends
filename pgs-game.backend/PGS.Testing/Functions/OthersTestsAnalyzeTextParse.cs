using game.Entity.Game;
using Lck.Utility.Extensions.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.Core.Helper;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.Testing
{
    [TestClass]
    public class OthersTestsAnalyzeTextParse : BaseTest
    {

        public PlayerRepository PlayerRepository { get; set; }

        public ForbiddenIPAddressRepository ForbiddenIPAddressRepository { get; set; }

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }


        /// <summary>
        /// 各種雜項測試、驗證
        /// </summary>
        [TestMethod]
        public void AnalyzeTextAndParse_Test()
        {
            //非公司開發電腦 不執行這組test
            if (IsNotDeveloperComputerName())
                return;

            //如果有標記 No Participate Event Merchant
            var excludeMerchantIDs = new List<int>
            {
                176730,
                177412,
                177340,
                178353,
                176938,
                176937,
                176940,
                177110,
                176939,
                177109,
                178190
            };

            //參考2024活動的商戶
            //參考2024活動的商戶
            string participantMerchantIDs = @"
175663
175955
175358
175650
175384
177272
177082
177271
177270
175392
175310
175586
175598
175628
175946
175602
175973
175382
175323
175327
176212
175894
175307
176213
176209
175571


175529
175531
175532
175534
175535
177057
175530
175533
175548
175546
175539
175541
175540
175542
175683
175930
175543
175549
175536
175538
176819
175544
177411
177293
176528
176527
176985
176984
177534
176419
176420
177004
176053
176054
176055
176056


175084
176184

177294
177241
177242
177066
176532
176726
176712
175516
176885
175278
176806
177149
176329
175284
176875
176361
177330
176884
176699
176573
177151
177316



176852
176920
176935
176936
176919



178336
178335
178190
177110
177109
177061
176940
176939
176938
176937
176936
176935
176920
176919
176889


178353
178275
177412
177340
177316
176730
176671
176670


";
            var mySplit = AppHelper.AnalyzeMerchantIDsByRegex(participantMerchantIDs);

            var mySplitAfterExclude = mySplit.Where(id => !excludeMerchantIDs.Contains(id)).ToList();

            string finalText = string.Join(",", mySplitAfterExclude);
            return;
        }


    }


}
