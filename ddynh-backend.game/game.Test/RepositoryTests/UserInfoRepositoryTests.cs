using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace game.Test
{
    [TestClass]
    public class UserInfoRepositoryTests
    {
        [TestMethod]
        public void GetUniversalUserInfoByUserIDs_Test()
        {
            var rep = new UserInfoRepository();

            List<int> Ids = new List<int>();

            for (int i = 0; i < 3000; i++)
            {
                Ids.Add(i);
            }

            Ids.Add(36433483);
            Ids.Add(15691440);
            Ids.Add(84465917);
            Ids.Add(10688215);
            Ids.Add(88101326);
            Ids.Add(68314452);

            var query = rep.GetUniversalUserInfoByUserIDs(Ids);
            return;
        }
    }
}
