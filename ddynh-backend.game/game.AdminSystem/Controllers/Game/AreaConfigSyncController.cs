using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Entity.Area;
using game.Infrastructure;
using game.Entity.AdminSys;
using FW.Core.Helper;
using DataTables.Mvc;
using Newtonsoft.Json;
using game.Infrastructure.Extensions;
using game.Entity;
using game.Entity.Game;

namespace game.AdminSystem.Controllers.Game
{
    public class AreaConfigSyncController : BaseController
    {
        private FW.Core.AdminSys.Provider.IAdminSysProvider _adminSysProvider;

        public AreaConfigSyncController(FW.Core.AdminSys.Provider.IAdminSysProvider adminSysProvider)
        {
            _adminSysProvider = adminSysProvider;
        }


        [NoAuthorizationAttribute]
        public ActionResult ShowAreasCheckboxes(int areaID)
        {
            List<AreaAdminUser> queryAllMyAreas = DataService.GetAllArea();

            var matchThis = queryAllMyAreas.FirstOrDefault(z => z.AreaID == areaID);

            if (matchThis == null)
                return PartialView(No_Permission_Page_View);

            ViewBag.SourceArea = matchThis;

            var querySamePlatform = queryAllMyAreas.Where(z => z.PlatformID == matchThis.PlatformID)
                .Where(z => z.AreaID != areaID)
                .ToList();

            List<int> othersAreaIds = querySamePlatform.Select(z => z.AreaID.Value).ToList();
            
            querySamePlatform = querySamePlatform
                .Where(z=> z.AreaID.HasValue)
                .ToList();

            return PartialView(querySamePlatform);
        }


        private List<int> GetAvailableAreasIfAgentSettingEqual(int sourceAreaID , List<int> othersAreaIDs)
        {
            var queryAllAreas = SiteHelper.GetAPIResult<List<Area>>("/Area/GetArea?isBriefOnly=true");

            var matchThis = ConvertToJsonObjectModel(queryAllAreas.Where(x => x.Id == sourceAreaID)).FirstOrDefault();
            
            var others = ConvertToJsonObjectModel(queryAllAreas.Where(x => othersAreaIDs.Contains(x.Id)));

            if (matchThis.AreaConfigSetting == null)
                return new List<int>();

            List<int> availableAreaIDs = new List<int>();

            if(matchThis.AreaConfigSetting != null)
            {
                var sourceSetting = matchThis.AreaConfigSetting.systemConfig.payConfig;

                //只允許相同代理設定的同步
                availableAreaIDs = others
                    .Where(x => x.AreaConfigSetting != null
                        && x.AreaConfigSetting.systemConfig.payConfig.UIClub2AgentMode1 == sourceSetting.UIClub2AgentMode1
                        && x.AreaConfigSetting.systemConfig.payConfig.UIClub2AgentMode3 == sourceSetting.UIClub2AgentMode3
                        && x.AreaConfigSetting.systemConfig.payConfig.UIClub2AgentMode4 == sourceSetting.UIClub2AgentMode4
                        && x.AreaConfigSetting.systemConfig.payConfig.UIClub2AgentMode5 == sourceSetting.UIClub2AgentMode5)
                    .Select(x => x.AreaID)
                    .ToList();
            }

            return availableAreaIDs;
        }


        private List<AreaConfigJsonModelTemp> ConvertToJsonObjectModel(IEnumerable<Area> areas)
        {
            return areas.Select(z => new AreaConfigJsonModelTemp
            {
                AreaID = z.Id,
                AreaConfigSetting = z.AreaConfig.TryJsonConvertToModel<AreaConfigJsonModel>()
            })                
            .ToList();
        }


        [NoAuthorizationAttribute]
        public ActionResult SyncConfigSubmit(string areaIds , int sourceAreaID)
        {
            if (string.IsNullOrEmpty(areaIds))
                return Json(new CRUDResultModel("areaIds 不得为空"), JsonRequestBehavior.AllowGet);

            if (sourceAreaID <= 0)
                return Json(new CRUDResultModel("sourceAreaID 不得为空"), JsonRequestBehavior.AllowGet);

            List<int> areaIdsConvert = areaIds.Split(',').Select(x => x).Where(z => !string.IsNullOrEmpty(z)).Select(z => Int32.Parse(z)).ToList();

            if (areaIdsConvert.IsEmptyOrNull())
                return Json(new CRUDResultModel("areaIdsConvert、AreaIds 不得为空"), JsonRequestBehavior.AllowGet);

            var postBody = new SyncAreaRequestModel
            {
                TemplateAreaID = sourceAreaID,
                TargetAreaIDs = areaIdsConvert
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>("Area/SyncAreaConfigToTargetAreaIDs", postBody);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);

        }

    }
}