using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    /// <summary>
    /// 快捷操作
    /// </summary>
    public class QuickAction
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id
        {
            get;
            set;
        }

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
