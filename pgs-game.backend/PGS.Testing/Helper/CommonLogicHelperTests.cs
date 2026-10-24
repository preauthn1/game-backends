using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class CommonLogicHelperTests : BaseTest
    {

        [TestMethod]
        public void HasIONSlotKeyword_Test()
        {
            Assert.IsTrue(CommonLogicHelper.HasIONSlotKeyword("ION X NXT"));
            Assert.IsTrue(CommonLogicHelper.HasIONSlotKeyword("Z181 WL21 (1XPR) x ION Slot"));
            Assert.IsTrue(CommonLogicHelper.HasIONSlotKeyword("API ION SLOT X SINAR INTEGRATION GROUP"));
            Assert.IsTrue(CommonLogicHelper.HasIONSlotKeyword("[Vendor] TG X target ION SLOT"));
            Assert.IsTrue(CommonLogicHelper.HasIONSlotKeyword("iOn sLot"));


            Assert.IsFalse(CommonLogicHelper.HasIONSlotKeyword("target X AsiaW Integration"));
            Assert.IsFalse(CommonLogicHelper.HasIONSlotKeyword("Play Station"));
            return;
        }


    }
}
