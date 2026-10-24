using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.Customer
{
    /// <summary>
    /// 快速回复类型
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class ReplyType : BaseEntity
    {
        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// 系统账号ID
        /// </summary>
        public int AdminID
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
    }   
}