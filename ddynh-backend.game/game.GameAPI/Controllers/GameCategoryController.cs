using FW.Core.Helper;
using game.Entity;
using game.Entity.Event;
using game.Entity.Game;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;

namespace game.GameAPI.Controllers
{
    public class GameCategoryController : BaseAPIController
    {
        private IGameCategoryService _service;

        private IGameCategorySettingService _gmeCategorySettingService;



        public GameCategoryController(
            IGameCategoryService service , 
            IGameCategorySettingService gmeCategorySettingService)
        {
            _service = service;
            _gmeCategorySettingService = gmeCategorySettingService;
        }



        [HttpGet]
        public List<GameCategoryEntity> GetGameCategoriesByFilter([FromUri]QueryGameCategoryRequestViewModel filter)
        {
            var query = _service.GetByFilter(filter);
            return query;
        }


        [HttpGet]
        public List<GameCategorySettingEntity> GetGameCategorySettingsByFilter([FromUri]QueryGameCategorySettingRequestViewModel filter)
        {
            var query = _gmeCategorySettingService.GetByFilter(filter);
            return query;
        }
        

        [HttpPost]
        public CRUDResultModel Create([FromBody]GameCategoryEntity model)
        {
            var query = _service.Create(model);
            return query;
        }
        

        [HttpPost, HttpPut]
        public CRUDResultModel EditCategories([FromBody]List<GameCategoryEntity> models)
        {
            var query = _service.EditMultiple(models);
            return query;
        }


        [HttpPost, HttpPut]
        public CRUDResultModel EditGameCategorySettings([FromBody]List<GameCategorySettingEntity> models)
        {
            var query = _gmeCategorySettingService.EditMultiple(models);
            return query;
        }
    }
}