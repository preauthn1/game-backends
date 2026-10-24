using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    /// <summary>
    /// 快捷操作
    /// </summary>
    [Database(DBNameConfig.DBName_AdminSys)]
    public class QuickAction : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 页面地址
        /// </summary>
        [Required]
        [StringLength(512)]
        public string Url
        {
            get;
            set;
        }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort
        {
            get;
            set;
        }

    }
}
