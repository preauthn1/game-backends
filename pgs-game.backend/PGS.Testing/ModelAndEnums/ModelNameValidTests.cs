using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class ModelNameValidTests : BaseTest
    {
        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }


        /// <summary>
        /// 確保某些model欄位名稱沒有被更改 不然可能會導致舊程式壞掉
        /// </summary>
        [TestMethod]
        public void ValidPropertyNameNotChanged()
        {
            //如更改此欄位，會導致db存的json 欄位無法匹配
            Assert.IsTrue(nameof(PerGameChangeSettleInfoParameter.BetAmount) == "BetAmount");
            Assert.IsTrue(nameof(PerGameChangeSettleInfoParameter.PayoutAmount) == "PayoutAmount");
            Assert.IsTrue(nameof(PerGameChangeSettleInfoParameter.UpdateAmount) == "UpdateAmount");
            Assert.IsTrue(nameof(PerGameChangeSettleInfoParameter.ValidAmount) == "ValidAmount");
            Assert.IsTrue(nameof(PerGameChangeSettleInfoParameter.TaxAmount) == "TaxAmount");
        }
		
        
    }
}
