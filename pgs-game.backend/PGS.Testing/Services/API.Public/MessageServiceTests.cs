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
    public class MessageServiceTests : BaseTest
    {

        public IMessageService ServiceInstance { get; set; }

        public MessageReadRepository MessageReadRepository { get; set; }


        [TestMethod]
        public void GetNotReadMessages_AndRead_Test()
        {
            int playerID = _TEST_DATA.Default.PlayerID;

            var query = ServiceInstance.GetPlayerMessages(playerID);

            var notReadMessages = query.Items.Where(z => !z.IsRead).ToList();
            if (notReadMessages.IsNotEmptyNull())
            {
                var matchFirst = notReadMessages.FirstOrDefault();
                long messageID = matchFirst.MessageID;

                ServiceInstance.SetMessageRead(playerID, new MessageReadRequestModel
                {
                    MessageID = messageID
                });

                //清除重置
                MessageReadRepository.DeleteByModel(new 
                {
                    MessageID = messageID,
                    PlayerID = playerID
                });
            }

            return;
        }

    }
}

