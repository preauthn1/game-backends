using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
    /// <summary>
    /// 问题处理进度表
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class QuestionHandleRecords:BaseEntity
    {
        /// <summary>
        /// 问题编号
        /// </summary>
        public int QsId
        {
            get;
            set;
        }
        /// <summary>
        /// 问题处理状态
        /// </summary>
        public HandleStatus QsHandleStatus
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
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserName
        {
            get;
            set;
        }

        public virtual QuestionList Question
        {
            get;
            set;
        }
    }
    /// <summary>
    /// 问题处理状态
    /// </summary>
    public enum HandleStatus:byte
    {
        [EnumDescription("新创建")]
        New,
        [EnumDescription("处理中")]
        Handle,
        [EnumDescription("已完成")]
        Finsh
    }
}
