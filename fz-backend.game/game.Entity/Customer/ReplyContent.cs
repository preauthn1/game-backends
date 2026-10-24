using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.Customer
{
    /// <summary>
    /// 快速回复内容
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class ReplyContent : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 回复内容
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// 快速回复类型ID
        /// </summary>
        public int ReplyTypeID
        {
            get;
            set;
        }

        /// <summary>
        /// 排序值
        /// </summary>
        public int Sort
        {
            get;
            set;
        }

        public virtual ReplyType ReplyType
        {
            get;
            set;
        }
    }   
}