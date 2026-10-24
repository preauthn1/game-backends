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
    public class LobbyPlayerTagServiceTests : BaseTest
    {

        public ILobbyPlayerTagService ServiceInstance { get; set; }


        [TestMethod]
        public void AddPlayerTagFromLobby_Test()
        {
            //AddPlayerTagFromLobby 已經棄用了

            //ServiceInstance.AddPlayerTagFromLobby(_TEST_DATA.Default.PlayerID , new LazyCommonRequestModel
            //{
            //    TagID = SystemTagEnum.Player_CurrencyExchangeHintReadFlag.ToInteger()
            //});

            return;
        }

    }
}

