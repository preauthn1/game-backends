
namespace game.Entity.Game
{
    public class HallVersionInfo
    {
        public int PlatformID { get; set; }

        public string PlatformName { get; set; }

        /// <summary>
        /// 安卓版本
        /// </summary>
        public string AndroidVersion { get; set; }

        /// <summary>
        /// iOS版本
        /// </summary>
        public string IOSVersion { get; set; }

        /// <summary>
        /// 安卓临时版本
        /// </summary>
        public string AndroidVersion2 { get; set; }

        /// <summary>
        /// iOS临时版本
        /// </summary>
        public string IOSVersion2 { get; set; }

        /// <summary>
        /// 大厅样版ID
        /// </summary>
        public HallSkinEnum HallSkinID { get; set; }

        public string HallSkinName { get; set; }
    }
}
