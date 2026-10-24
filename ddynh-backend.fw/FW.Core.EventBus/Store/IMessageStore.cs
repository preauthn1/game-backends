using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.EventBus.Store
{
    /// <summary>
    /// 消息持久化接口
    /// </summary>
    public interface IMessageStore
    {
        /// <summary>
        /// 保存消息
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool SaveMessage(EventMessage msg);

        /// <summary>
        /// 根据条件获取消息
        /// </summary>
        /// <param name="classID"></param>
        /// <param name="from"></param>
        /// <param name="end"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<EventMessage> GetMessage(List<int> classID, DateTime? from, DateTime? end, int? count);

    }
}
