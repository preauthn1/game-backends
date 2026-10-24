using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using game.Entity.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    
    public class SyncAreaRequestModel 
    {       

        public int TemplateAreaID { get; set; }


        public List<int> TargetAreaIDs { get; set; }


        public AreaConfigSyncOptionalSettingModel OptionalSetting { get; set; }

    }
}
