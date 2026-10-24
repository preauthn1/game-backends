using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels
{

    public class PromotionEventViewModel
    {

        public List<PromotionEventItemViewModel> Events { get; set; }

    }






    public class PromotionEventItemViewModel
    {

        public int EventID { get; set; }

        public string EventName { get; set; }

        /// <summary>
        /// ApplyEventTypeEnum.cs 請參考
        /// </summary>
        public int EventType { get; set; }
        

        public DateTime StartTime { get; set; }


        public DateTime EndTime { get; set; }

    }




    public class PromotionEventDetailViewModel
    {

        public PromotionEventItemViewModel BasicInfo { get; set; }
        

        /// <summary>
        /// 轉盤特有的欄位
        /// </summary>
        public PromotionEventLuckyRouletteExtrViewModel LuckyRouletteInfo { get; set; }

    }




    public class BaseUserInfoRequestViewModel
    {

        public int UserID { get; set; }


        public string Certificate { get; set; }

    }



    public class PromotionEventRequestViewModel : BaseUserInfoRequestViewModel
    {

    }


    public class PromotionEventDetailRequestViewModel : BaseUserInfoRequestViewModel
    {

        public int EventID { get; set; }


    }


    public class PromotionEvenBonusDrawRequestViewModel : BaseUserInfoRequestViewModel
    {

        public int EventID { get; set; }


    }



}
