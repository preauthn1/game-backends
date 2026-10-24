namespace game.Entity.Area
{
    public class AreaAdminUser
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 渠道所属管理员ID
        /// </summary>
        public int AdminID { get; set; }


        /// <summary>
        /// 渠道ID
        /// </summary>
        public int? AreaID { get; set; }


        /// <summary>
        /// 渠道
        /// </summary>
        public string AreaName { get; set; }
    }
}
