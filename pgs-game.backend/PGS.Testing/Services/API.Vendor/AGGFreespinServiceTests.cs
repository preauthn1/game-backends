using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class AGGFreespinServiceTests : BaseTest
    {

        public GameListRepository GameListRepository { get; set; }


        [TestMethod]
        public void GetSupportFreespinAGGGames_Test()
        {
            var freespinService = new AGGFreespinService();

            var aggGames = GameListRepository.GetByFilter(new Models.ViewModels.QueryGameListRequestViewModel
            {
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameVendorTypes = GameVendorTypeEnum.AGG.ToInteger().ToListCollection()
            });

            var queryGames = freespinService.GetSupportFreespinAGGGames();

            Assert.IsTrue(queryGames.Count() >= 20 && queryGames.Count() <= (aggGames.Count() - 10));
            return;
        }

    }
}

