using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
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
    public class TelegramAnnouncementComplexServiceTests : BaseTest
    {

        public ITelegramAnnouncementComplexService ServiceInstance { get; set; }


        [TestMethod]
        public void GetDefaultContentByTemplateID_Test()
        {
            var query1 = ServiceInstance.GetDefaultContentByTemplateID(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                TemplateID = TelegramAnnouncementTemplateEnum.MaintainmAnouncement.ToInteger()
            });

            var query2 = ServiceInstance.GetDefaultContentByTemplateID(_TEST_DATA.Default.UserID, new LazyCommonRequestModel
            {
                TemplateID = TelegramAnnouncementTemplateEnum.NewGameRelease.ToInteger(),
                GameID = _TEST_DATA.Default.GameID
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
            return;
        }



        [TestMethod]
        public void ReplacePGSKeywordToIONSLOT_Test()
        {
            string originalMsg1 = "target 明日即將維護";
            string originalMsg2 = "target Maintain 公告";
            string originalMsg3 = "ION SLOT Maintain 公告";
            string originalMsg4 = "PAS Maintain 公告";

            string newMsg1 = ServiceInstance.ReplacePGSKeywordToIONSLOT(originalMsg1);
            string newMsg2 = ServiceInstance.ReplacePGSKeywordToIONSLOT(originalMsg2);
            string newMsg3 = ServiceInstance.ReplacePGSKeywordToIONSLOT(originalMsg3);
            string newMsg4 = ServiceInstance.ReplacePGSKeywordToIONSLOT(originalMsg4);

            Assert.IsTrue(!newMsg1.Contains("target") && newMsg1.Contains("ION SLOT"));
            Assert.IsTrue(!newMsg2.Contains("target") && newMsg2.Contains("ION SLOT"));

            Assert.IsTrue(newMsg1.CountKeywordMatchTimes("ION SLOT") == 1);
            Assert.IsTrue(newMsg2.CountKeywordMatchTimes("ION SLOT") == 1);
            Assert.IsTrue(newMsg4.CountKeywordMatchTimes("ION SLOT") == 0);

            Assert.IsTrue(newMsg3.Contains("ION SLOT"));
            Assert.IsTrue(originalMsg4 == newMsg4);
            return;
        }



        [TestMethod]
        public void ReplacePGSKeywordToOtherText_PGSKeyword_Test()
        {
            string originalMsg1 = "target 明日即將維護";
            string originalMsg2 = "target Maintain 公告";
            string originalMsg3 = "【target Maintain 公告】";
            string originalMsg4 = "target 維護公告";

            string replaceToNewText = "Avatar／target";
            var myService = new TelegramAnnouncementService();

            string newMsg1 = myService.ReplacePGSKeywordToOtherText(originalMsg1, replaceToNewText);
            string newMsg2 = myService.ReplacePGSKeywordToOtherText(originalMsg2, replaceToNewText);
            string newMsg3 = myService.ReplacePGSKeywordToOtherText(originalMsg3, replaceToNewText);
            string newMsg4 = myService.ReplacePGSKeywordToOtherText(originalMsg4, replaceToNewText);

            Assert.IsTrue(newMsg1.CountKeywordMatchTimes("target") == 1);
            Assert.IsTrue(newMsg2.CountKeywordMatchTimes("target") == 1);
            Assert.IsTrue(newMsg3.CountKeywordMatchTimes("target") == 1);
            Assert.IsTrue(newMsg4.CountKeywordMatchTimes("target") == 1);
            return;
        }


        [TestMethod]
        public void SendTGNotifyByAPI_Test()
        {
            ServiceInstance.SendTGNotifyByAPI(_TEST_DATA.Default.UserID, new TelegramAnnouncementSubmitViewModel
            {
                SendTargetsByTagID = _TEST_DATA.Default.IMChannelTagID,
                BannerImageUrl = "https://example.invalid/",
                Content = "內文123",
                TemplateID = TelegramAnnouncementTemplateEnum.MaintainmAnouncement.ToInteger(),
                Buttons = new List<TelegramAnnouncementButtonItemModel>
                {
                    new TelegramAnnouncementButtonItemModel
                    {
                        IsDisabled = false,
                        LinkUrl = "https://example.invalid/",
                        Text = "按鈕1"
                    }
                }
            });

            return;
        }

    }
}

