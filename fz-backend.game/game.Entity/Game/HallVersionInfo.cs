
namespace game.Entity.Game
{
    public class HallVersionInfo
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public string PlatformID { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 安卓版本
        /// </summary>
        public string AndroidVersion
        {
            get;
            set;
        }
        /// <summary>
        /// iOS版本
        /// </summary>
        public string IOSVersion
        {
            get;
            set;
        }
        /// <summary>
        /// 安卓临时版本
        /// </summary>
        public string AndroidVersion2
        {
            get;
            set;
        }
        /// <summary>
        /// iOS临时版本
        /// </summary>
        public string IOSVersion2
        {
            get;
            set;
        }
    }
}
