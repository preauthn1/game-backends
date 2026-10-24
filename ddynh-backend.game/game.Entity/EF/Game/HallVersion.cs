using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    public class HallVersion
    {
        public string AndroidVersion { get; set; }

        public string IOSVersion { get; set; }

        /// <summary>
        /// 安卓临时版本
        /// </summary>
        public string AndroidVersion2 { get; set; }

        /// <summary>
        /// iOS临时版本
        /// </summary>
        public string IOSVersion2 { get; set; }

        public HallSkinEnum HallSkinID { get; set; }

        public int PlatformID { get; set; }
    }

    public class HallVersionList
    {
        /// <summary>
        /// 安卓版本
        /// </summary>
        public List<string> AndroidVersion { get; set; }

        /// <summary>
        /// iOS版本
        /// </summary>
        public List<string> IOSVersion { get; set; }

        /// <summary>
        /// 安卓临时版本
        /// </summary>
        public List<string> AndroidVersion2 { get; set; }

        /// <summary>
        /// iOS临时版本
        /// </summary>
        public List<string> IOSVersion2 { get; set; }

        public List<string> Skin { get; set; }

        public List<int> PlatformID { get; set; }
    }
}
