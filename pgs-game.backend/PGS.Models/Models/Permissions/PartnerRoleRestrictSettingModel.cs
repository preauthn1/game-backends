using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class PartnerRoleRestrictSettingModel
    {
        public List<int> CanOnlyQueryAGG_RoleIDs { get; set; }

        public List<int> CanOnlyQueryAGGLive_RoleIDs { get; set; }

        public List<int> CanOnlyQueryAGGFish_RoleIDs { get; set; }
    }
}
