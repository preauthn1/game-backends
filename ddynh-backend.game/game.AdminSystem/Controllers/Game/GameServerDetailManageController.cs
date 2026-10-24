using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataTables.Mvc;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Users;
using game.Infrastructure;
using Newtonsoft.Json;
using game.AdminSystem.Models.ViewModels;
using game.Infrastructure.Extensions;
using FW.Core.Helper;
using game.Service.DAO;

namespace game.AdminSystem.Controllers.Game
{
   
    public class GameServerDetailManageController : BaseController
    {

        public ServerPlatformRelationsRepository ServerPlatformRelationsRepository { get; set; }



        [NoAuthorization]
        public ActionResult EditRelationPlatforms(int serverID)
        {
            var queryGameServer = SiteHelper.GetAPIResult<List<GameServer>>($"Game/GetGameServer?serverID={serverID}").FirstOrDefault();

            if (queryGameServer == null)
                return Alert("查无资料");

            var viewModel = new GameServerRelationViewModel
            {
                ServerID = serverID,
                ServerName = queryGameServer.ServerName,
                ShowName = queryGameServer.ShowName,
            };
            return View(viewModel);
        }


        [NoAuthorization]
        public ActionResult AjaxGetSettings(int serverID)
        {
            var queryGameServer = SiteHelper.GetAPIResult<List<GameServer>>($"Game/GetGameServer?serverID={serverID}").FirstOrDefault();

            var queryMapping = SiteHelper.GetAPIResult<List<ServerPlatformRelationsEntity>>("ServerInfo/GetAllServerPlatformRelations");

            var queryAllPlatforms = DataService.GetAllPlatform();

            queryMapping = queryMapping.Where(x => x.ServerID == serverID).ToList();

            var settings = queryAllPlatforms.Select(x =>
            {
                var convert = x.ReturnInjecteObject<GameServerRelationSettingItemViewModel>();
                convert.IsActive = queryMapping.Any(z => z.PlatformID == x.Id && z.ServerID == serverID && z.Status == (int)NormalActiveStatusEnum.Enable);

                return convert;

            })
            .ToList();

            return Json(settings, JsonRequestBehavior.AllowGet);
        }
        
        [NoAuthorization]
        public ActionResult CreateOrUpdateStatus(UpdateServerRelationRequestModel model)
        {            
            var result = SiteHelper.PostGameApi<CRUDResultModel>("ServerInfo/CreateOrUpdateStatus" , model);
            return Json(result.ResultObject , JsonRequestBehavior.AllowGet);
        }



        [NoAuthorization]
        public ActionResult ReverseCurrentStatus(int serverID  , int platformID)
        {
            var queryExist = ServerPlatformRelationsRepository.GetByUniqueCondition(platformID, serverID);

            int afterStatus = NormalActiveStatusEnum.Enable.ToInteger();

            if(queryExist != null)
            {
                if(queryExist.Status == NormalActiveStatusEnum.Enable.ToInteger())
                {
                    afterStatus = NormalActiveStatusEnum.Disable.ToInteger();
                }
                else
                {
                    afterStatus = NormalActiveStatusEnum.Enable.ToInteger();
                }
            }

            var model = new UpdateServerRelationRequestModel
            {
                ServerID = serverID,
                PlatformID = platformID,
                TargetStatus = afterStatus
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>("ServerInfo/CreateOrUpdateStatus", model);
            
            result.ResultObject.ResultObject =  new 
            {
                afterStatus = afterStatus
            };
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult CopyServerRelationFromTemplate(int targetPlatformID , int templatePlatformID)
        {
            if (targetPlatformID <= 0 || templatePlatformID <= 0)
                return Json(new CRUDResultModel("请选择平台"), JsonRequestBehavior.AllowGet);

            if (targetPlatformID == templatePlatformID)
                return Json(new CRUDResultModel("来源与目标不得相同"), JsonRequestBehavior.AllowGet);

            var queryExistSetting = ServerPlatformRelationsRepository.GetByPlatformID(targetPlatformID);
                         
            //為了避免選反 防呆
            if(queryExistSetting.Count() >= 20)
                return Json(new CRUDResultModel($"您选择的复制目标: {targetPlatformID}，资料已超过20笔，为避免误删资料将禁止此操作。仅支援手动处理"), JsonRequestBehavior.AllowGet);

            ServerPlatformRelationsRepository.CopyServerRelationByTemplatePlatform(targetPlatformID, templatePlatformID);
            return Json(new CRUDResultModel(true, "复制成功！"), JsonRequestBehavior.AllowGet);
        }


    }
}