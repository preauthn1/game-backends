using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class Right : BaseEntity
    {
        public int RightID
        {
            get { return Id; }
            set { Id = value; }
        }

        [Required]
        [StringLength(32)]
        public string RightName
        {
            get;
            set;
        }

        /// <summary>
        /// 父级权限ID
        /// </summary>
        public int ParentID
        {
            get;
            set;
        }

        /// <summary>
        /// 权限标识
        /// </summary>
        [Required]
        [StringLength(128)]
        public string RightToken
        {
            get;
            set;
        }

        /// <summary>
        /// 页面地址
        /// </summary>
        [StringLength(512)]
        public string PageUrl
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

        /// <summary>
        /// 权限类型
        /// </summary>
        public RightType RightType
        {
            get;
            set;
        }

        /// <summary>
        /// 是否在菜单中显示
        /// </summary>
        public bool ShowInMenu
        {
            get;
            set;
        }

        /// <summary>
        /// 图标样式
        /// </summary>
        [StringLength(32)]
        public string IconCls
        {
            get;
            set;
        }

    }

    public enum RightType : byte
    {
        PageRight,
        CommandRight
    }
}
