using game.Entity.Game;
using game.Entity.ViewModels;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Web.Http;

namespace Job.API.BG.Controllers
{
    public class DataMigrateController : BaseController
    {

        public IDataMigrateService DataMigrateService { get; set; }



        [Route("flow-amount/calculate")]
        [HttpPost, HttpGet]
        public object CalculateFlowAmountProcess()
        {
            DataMigrateService.CalculateFlowAmountProcess((int)GameMerchantEnum.BG, (int)GameListEnum.BG_Casino);
            return "OK";
        }


        [Route("game-log/transform")]
        [HttpPost, HttpGet]
        public object TransformDataToGameLog()
        {
            DataMigrateService.TransformDataToGameLog((int)GameMerchantEnum.BG, (int)GameListEnum.BG_Casino);
            return "OK";
        }


    }
}
