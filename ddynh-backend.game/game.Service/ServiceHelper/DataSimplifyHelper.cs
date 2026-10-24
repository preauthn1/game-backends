using game.Entity.Area;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Helper
{
    public class DataSimplifyHelper
    {
        /// <summary>
        /// 清空string，掉很佔傳輸空間的欄位(實際上也沒用到)
        /// </summary>
        /// <param name="area"></param>
        public static void SimplifyAreaContent(Area area)
        {
            if (area == null)
                return;

            area.FAQContent = string.Empty;
            area.PayTips = string.Empty;
            area.AreaConfig = string.Empty;

            SimplifyPlatformContent(area.Platform);
        }

        /// <summary>
        /// 清空string，掉很佔傳輸空間的欄位 (AreaConfig那欄會保留)
        /// </summary>
        /// <param name="area"></param>
        public static void SimplifyAreaContentAndTips(Area area)
        {
            if (area == null)
                return;

            area.FAQContent = string.Empty;
            area.PayTips = string.Empty;

            SimplifyPlatformContent(area.Platform);
        }




        public static void SimplifyPlatformContent(Platform platform)
        {
            if (platform == null)
                return;

            platform.FAQContent = string.Empty;
            platform.PayTip = string.Empty;
        }

        public static void SimplifyPlatformContent(IEnumerable<Platform> platforms)
        {
            foreach(var item in platforms)
            {
                SimplifyPlatformContent(item);
            }
        }

    }
}
