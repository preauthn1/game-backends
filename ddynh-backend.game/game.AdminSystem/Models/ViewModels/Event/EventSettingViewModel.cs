using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using game.Infrastructure.Extensions;
using game.Entity.Money;

namespace game.AdminSystem.Models.ViewModels
{
    
    public class EventSettingViewModel : EventSettingEntity
    {
        public string EventTypeText { get; set; }

        public string StatusName { get; set; }

        public string PlatformName { get; set; }

        public string StartTimeText { get; set; }

        public string EndTimeText { get; set; }

        public string UpdateTimeText { get; set; }

        public bool IsExpired { get; set; }
    }
}