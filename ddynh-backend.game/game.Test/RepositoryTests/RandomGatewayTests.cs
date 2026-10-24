using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace game.Test
{
    [TestClass]
    public class RandomGatewayTests
    {
        [TestMethod]
        public void GetRandomGatewayByPriority_Test()
        {
            return;


            InitailHelper.InitializeIOC();

            var rep = new PlatformPayTypeSettingRepository();


            var payType = Entity.Pay.PayType.WeixinFixed;
            int platform = 6000;
            int amount = 1000;

            int maxTimes = 50;

            List<RandomGatewayResultModel> result = new List<RandomGatewayResultModel>();

            for (int i = 0; i < maxTimes; i++)
            {
                var sp_result = rep.GetRandomGatewayByPriority(payType, platform, amount);
                result.Add(sp_result);
            }

            var qroupBy = result.GroupBy(x =>$"{x.TypeID}_{x.GWID}" ).Select(x => new
            {
                key = x.Key,
                count = x.Count()
            })
            .ToList();


            return;
        }
    }
}
