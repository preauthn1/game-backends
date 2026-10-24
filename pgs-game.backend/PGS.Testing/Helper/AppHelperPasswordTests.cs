using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.Models;
using target.Utility.Helper;

namespace target.Testing
{
    [TestClass]
    public class AppHelperPasswordTests
    {
        [TestMethod]
        public void ShwoAdminSysVisiblePassowrd_DefaultPassword_Tests()
        {
            string passwordHash = AppHelper.GeneratePasswordHash(SystemConstant.Default_EazyPassword);

            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, 175001);

            Assert.AreEqual(SystemConstant.Default_EazyPassword, result);
        }


        [TestMethod]
        public void ShwoAdminSysVisiblePassowrd_LegacyDefaultPassword_Tests()
        {
            string passwordHash = AppHelper.GeneratePasswordHash("aaa111");

            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, 175001);

            Assert.AreEqual(SystemConstant.Default_EazyPassword, result);
        }


        [DataTestMethod]
        [DataRow(175001)]
        [DataRow(175002)]
        public void ShwoAdminSysVisiblePassowrd_MerchantIDPassword_Tests(int merchantID)
        {
            string password = [REDACTED];
            string passwordHash = AppHelper.GeneratePasswordHash(password);

            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, merchantID);

            Assert.AreEqual(password, result);
        }


        [DataTestMethod]
        [DataRow("test001")]
        [DataRow("MYC-test001")]
        public void ShwoAdminSysVisiblePassowrd_UserNamePassword_Tests(string userName)
        {
            string password = [REDACTED];
            string passwordHash = AppHelper.GeneratePasswordHash(password);

            var result = AppHelper.ShwoAdminSysVisiblePassowrd(userName, passwordHash, 175001);

            Assert.AreEqual(password, result);
        }


        [DataTestMethod]
        [DataRow("MYC-test001", "test001a")]
        [DataRow("test001", "test001a")]
        public void ShwoAdminSysVisiblePassowrd_AMPassword_Tests(string userName, string password)
        {
            string passwordHash = AppHelper.GeneratePasswordHash(password);

            var result = AppHelper.ShwoAdminSysVisiblePassowrd(userName, passwordHash, 175001);

            Assert.AreEqual(password, result);
        }


        [TestMethod]
        public void ShwoAdminSysVisiblePassowrd_CustomPassword_Tests()
        {
            string passwordHash = AppHelper.GeneratePasswordHash("CustomPassword!2026");

            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, 175001);

            Assert.AreEqual(string.Empty, result);
        }


        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("INVALID_PASSWORD_HASH")]
        public void ShwoAdminSysVisiblePassowrd_UnrecognizedPasswordHash_Tests(string passwordHash)
        {
            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, 175001);

            Assert.AreEqual(string.Empty, result);
        }


        [TestMethod]
        public void ShwoAdminSysVisiblePassowrd_DifferentMerchantID_Tests()
        {
            string password = [REDACTED];
            string passwordHash = AppHelper.GeneratePasswordHash(password);

            var result = AppHelper.ShwoAdminSysVisiblePassowrd("test001", passwordHash, 175002);

            Assert.AreEqual(string.Empty, result);
        }
    }
}
