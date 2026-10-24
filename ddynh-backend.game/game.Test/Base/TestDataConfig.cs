using System;
using System.Collections.Generic;
using game.Entity;
using game.Service.DAO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using game.Infrastructure.Extensions;
using System.Reflection;
using FW.Core.Engine;
using game.Infrastructure;

namespace game.Test
{

    public class TestDataConfig
    {
        public TestDataGenerateModel Default = new TestDataGenerateModel();
    }


    public class TestDataGenerateModel
    {
        public int PlatformID = 1000;

        public int AreaID = 1000001;

        public int UserID = 34290561;

        public int AdminUserID = 1;

        public string Certificate = SystemConst.MAGIC_CODE;
    }


}
