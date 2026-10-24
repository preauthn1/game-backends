
using FW.Core.Helper.Enum;

namespace game.Entity.Area
{
    public class AreaGame
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 渠道ID
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 游戏名称
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 开关状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 熱門程度
        /// </summary>
        public int Popular { get; set; }

        /// <summary>
        /// 熱門程度顯示名稱
        /// </summary>
        public string PopularName { get; set; }
    }

   
}
