using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Core.Helper;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class SQLHelperTests : BaseTest
    {



        [TestMethod]
        public void GetInsertCommand_Test()
        {
            var updateSQL = SQLHelper.GetInsertCommand<MyTestFakeEntity>("XXXXXXXX");

            Assert.IsTrue(updateSQL.Contains("CreateBy"));
            Assert.IsTrue(updateSQL.Contains("CreateTime"));
            Assert.IsTrue(updateSQL.Contains("Name"));
            Assert.IsTrue(updateSQL.Contains("Memo"));
            Assert.IsFalse(updateSQL.Contains("TableID"));

            return;

        }



        [TestMethod]
        public void GetInsertCommand_IgnoreColumn_Test()
        {
            var updateSQL = SQLHelper.GetInsertCommand<ApiRequestLogEntity>("ApiRequestLogEntity");

            Assert.IsTrue(updateSQL.Contains(nameof(ApiRequestLogEntity.CreateTime)));
            Assert.IsFalse(updateSQL.Contains("ExtraDebug_ErrorCode"));

            var SQL_2 = SQLHelper.GetFieldListCollection<ApiRequestLogEntity>(null);

            Assert.IsTrue(SQL_2.Contains(nameof(ApiRequestLogEntity.CreateTime)));
            Assert.IsFalse(SQL_2.Contains("ExtraDebug_ErrorCode"));
        }



        [TestMethod]
        public void GenerateInsertCommand_MultipleTimeIsEqual_Test()
        {
            string tableName = "BetRecordAGGLive";
            var insertSQL1 = SQLHelper.GetInsertCommand<BetRecordAGGLiveEntity>(tableName);
            var insertSQL2 = SQLHelper.GetInsertCommand<BetRecordAGGLiveEntity>(tableName);
            var insertSQL3 = SQLHelper.GetInsertCommand<BetRecordAGGLiveEntity>(tableName);
            var insertSQL4 = SQLHelper.GetInsertCommand<BetRecordAGGLiveEntity>(tableName);

            //確保每一次產生的sql都是一樣的 
            Assert.IsTrue(insertSQL1 == insertSQL2 && insertSQL1 == insertSQL3 && insertSQL1 == insertSQL4);

            var updateSQL1 = SQLHelper.GetUpdateCommandWithoutWhere<BetRecordAGGLiveEntity>(tableName);
            var updateSQL2 = SQLHelper.GetUpdateCommandWithoutWhere<BetRecordAGGLiveEntity>(tableName);
            var updateSQL3 = SQLHelper.GetUpdateCommandWithoutWhere<BetRecordAGGLiveEntity>(tableName);
            var updateSQL4 = SQLHelper.GetUpdateCommandWithoutWhere<BetRecordAGGLiveEntity>(tableName);

            Assert.IsTrue(updateSQL1 == updateSQL2 && updateSQL1 == updateSQL3 && updateSQL1 == updateSQL4);
            return;
        }



        [TestMethod]
        public void GetUpdateCommand_Test()
        {
            var updateSQL = SQLHelper.GetUpdateCommand<MyTestFakeEntity>("XXXXXXXX");

            Assert.IsFalse(updateSQL.UpdateColumnSQL.Contains("CreateBy"));
            Assert.IsFalse(updateSQL.UpdateColumnSQL.Contains("CreateTime"));
            Assert.IsTrue(updateSQL.UpdateColumnSQL.Contains("Name"));
            Assert.IsTrue(updateSQL.UpdateColumnSQL.Contains("Memo"));
            Assert.IsTrue(updateSQL.WhereSQL.Contains("TableID"));

            var debug = SQLHelper.GetUpdateCommand<BetRecordAGGEntity>("XXXXXXXX");

            return;

        }


        public class MyTestFakeEntity
        {
            [Key]
            public int TableID { get; set; }

            public string Name { get; set; }

            public string Memo { get; set; }

            [DoNotUpdate]
            public string CreateBy { get; set; }

            [DoNotUpdate]
            public DateTime CreateTime { get; set; }

            public DateTime UpdateTime { get; set; }

        }


    }
}
