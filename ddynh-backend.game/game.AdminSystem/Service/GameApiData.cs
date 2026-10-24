using FW.Core.Helper;
using game.Entity.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem
{
    public static class GameApiData
    {
       
        public static List<Organization> GetOrganizationByIds(List<int> orgIds)
        {
            string idValues = string.Join(",", orgIds);

            var apiResult = SiteHelper.GetAPIResult<List<Organization>>($"Organization/GetByOrgByIds?ids={idValues}");
            return apiResult;
        }


    }
}