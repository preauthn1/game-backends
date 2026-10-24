using game.Entity.Area;
using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
    
    public class UpdateEventAreaMappingViewModel
    {
        public List<Area> CurrentPlatformAreas { get; set; }

        public List<PromotionEventAreaMappingEntity> CurrentSettings { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; }
    }



}