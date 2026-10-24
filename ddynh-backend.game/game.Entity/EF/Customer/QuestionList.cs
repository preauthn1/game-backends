using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
    /// <summary>
    /// 客户问题
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class QuestionList:BaseEntity
    {
        /// <summary>
        /// 问题类型
        /// </summary>
        public int QsTypeId
        {
            get;
            set;
        }

        /// <summary>
        /// 问题标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端类型
        /// </summary>
        public ClientType ClientType
        {
            get;
            set;
        }

        /// <summary>
        /// 来源
        /// </summary>
        public OriginType Origin
        {
            get;
            set;
        }

        /// <summary>
        /// 详情内容
        /// </summary>
        public string ChildTypeFiledsContent
        {
            get;
            set;
        }
        /// <summary>
        /// 用户编号
        /// </summary>
        public int? UserID { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>
        [StringLength(20)]
        public string ContactName
        {
            get;
            set;
        }
        /// <summary>
        /// 联系人电话
        /// </summary>
        [StringLength(128)]
        public string ContactPhone
        {
            get;
            set;
        }
        /// <summary>
        /// 创建人名称
        /// </summary>
        [Required]
        [StringLength(20)]
        public string CreateUserName
        {
            get;
            set;
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// 处理状态
        /// </summary>
        public HandleStatus HandleStatus
        {
            get;
            set;
        }
        
        /// <summary>
        /// 分站编号
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }

        public virtual Customer.QuestionSites Site
        {
            get;
            set;
        }
        public virtual game.Entity.Customer.QuestionType QuestionType
        {
            get;
            set;
        }
        public virtual game.Entity.Users.UserInfo UserInfo
        {
            get;
            set;
        }
    }
}
