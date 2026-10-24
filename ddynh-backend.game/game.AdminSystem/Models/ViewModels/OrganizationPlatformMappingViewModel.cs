using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Area;

namespace game.AdminSystem
{
    public class OrganizationPlatformMappingViewModel
    {
        public int OrgId { get; set; }

        public string OrgName { get; set; }

        public List<int> SelectedPlatformIds { get; set; }

        public List<PlatformChangeViewModel> Platforms { get; set; }


    }



    public class PlatformChangeViewModel
    {
        public int Id { get; set; }


        public string Name { get; set; }


        public int MappingOrgCount { get; set; }
    }


}