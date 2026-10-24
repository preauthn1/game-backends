using game.Entity.Game;
using game.Infrastructure;
using System.Collections.Generic;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class WinAnnConfigController : BaseController
    {
        public ActionResult List()
        {
            return View(SiteHelper.GetAPIResult<List<WinAnnConfig>>("/Game/GetWinAnnConfig"));
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(WinAnnConfig model)
        {
            var result = SiteHelper.PostAPIResult<WinAnnConfig>("/Game/AddWinAnnConfig", model);
            if (result.Result)
                return ConfirmReturnList("List");
            else
                return Alert(result.Message);
        }
        
        public ActionResult Edit(int id)
        {
            var area = SiteHelper.GetAPIResult<WinAnnConfig>(string.Format("/Game/GetWinAnnConfigById?gameID={0}", id));
            if (area == null)
                return Alert("配置不存在");

            return View(area);
        }

        [HttpPost]
        public ActionResult Edit(WinAnnConfig model)
        {
            var result = SiteHelper.PostAPIResult<WinAnnConfig>("/Game/ModifyWinAnnConfig", model);
            if (result.Result)
                return ConfirmReturnList("List");
            else
                return Alert(result.Message);
        }

        public ActionResult Delete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/DelWinAnnConfig?id={0}", id));
            return Alert(result.Message, "/WinAnnConfig/List");
        }
    }
}