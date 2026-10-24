using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class BaseDapperRepositoryTests : BaseTest
    {

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }


        [TestMethod]
        public void ConvertClassToDapperDynamicParameters_FilterColumnOnly_Test()
        {
            var myObj = new
            {
                Id = 123,
                Name = "5555"
            };

            string sql = " SELECT * FROM AAA Where Id = @Id ";
            var convert = BetRecordExtraInfoRepository.ConvertClassToDapperDynamicParameters(myObj, sql);                                   

            Assert.IsTrue(convert.ParameterNames.Count() == 1);
            Assert.IsTrue(convert.ParameterNames.Contains("Id"));
            return;
        }


        [TestMethod]
        public void ConvertClassToDapperDynamicParameters_BetRecordExtraInfoEntity_Test()
        {
            var myObj = new BetRecordExtraInfoEntity
            {
                BetRecordNoPGS = "123",
            };

            bool isColumnMatch_1 = false;
            bool isColumnMatch_2 = false;

            var convert = BetRecordExtraInfoRepository.ConvertClassToDapperDynamicParameters(myObj);

            Dictionary<string, int> dic = new Dictionary<string, int>();

            foreach (var name in convert.ParameterNames)
            {
                var pValue = convert.Get<dynamic>(name);
                var dbStringType = pValue as DbString;

                if(name == "BetRecordNoPGS")
                {
                    Assert.IsNotNull(dbStringType);
                    Assert.IsTrue(dbStringType.Length == 32);
                    Assert.IsTrue(dbStringType.IsAnsi == false);
                    isColumnMatch_1 = true;
                }

                if (name == "ExtraInfo")
                {
                    Assert.IsNotNull(dbStringType);
                    Assert.IsTrue(dbStringType.Length >= 500);
                    Assert.IsTrue(dbStringType.IsAnsi == false);
                    isColumnMatch_2 = true;
                }

                dic.Add(name, 0);
            }

            Assert.IsTrue(isColumnMatch_1);
            Assert.IsTrue(isColumnMatch_2);

            return;
        }



        [TestMethod]
        public void GenerateInQuery_Or_LargeIDsQuery_Test()
        {
            List<int> playerIDs = new List<int>();
            string whereColumns = " @playerIDs ";

            for(int i = 0; i < 3333; i++)
            {
                playerIDs.Add(10000000 + i);
            }

            var result1 = BetRecordExtraInfoRepository.GenerateInQuery_Or_LargeIDsQuery(whereColumns, playerIDs);
            var result2 = BetRecordExtraInfoRepository.GenerateInQuery_Or_LargeIDsQuery(whereColumns, new List<int> {  1 , 2, 3});

            Assert.IsTrue(result1.TempIDsAppendSQL.Length > 500);
            Assert.IsTrue(result1.WhereSQL.Contains("tempIDs"));
            Assert.IsFalse(result1.WhereSQL.Contains("playerIDs"));

            Assert.IsTrue(string.IsNullOrWhiteSpace(result2.TempIDsAppendSQL));
            Assert.IsFalse(result2.WhereSQL.Contains("tempIDs"));
            Assert.IsTrue(result2.WhereSQL.Contains("playerIDs"));

            return;
        }


    }
}

