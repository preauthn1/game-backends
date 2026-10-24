using FW.Core.Helper;
using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class PromotionEventFullInfoViewModel : PromotionEventEntity
    {        

        public int TotalAreaCount { get; set; }


        public List<PromotionEventItemEntity> EventItems { get; set; }
       
    }       
}
