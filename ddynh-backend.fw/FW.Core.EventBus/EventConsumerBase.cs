using FW.Core.Helper;
using FW.Core.Helper.Thread;
using FW.Core.Helper.Trace;
using FW.Core.Logger;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FW.Core.EventBus
{
    /// <summary>
    /// 消费者基类
    /// </summary>
    public abstract class EventConsumerBase : IEventConsumer
    {
        protected ITrace Trace = null;
        private ReaderWriterLockSlim _dispatcherLocker = new ReaderWriterLockSlim();
        
        /// <summary>
        /// ctor
        /// </summary>
        public EventConsumerBase()
        {
            Trace = new EmptyTrace();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tarce"></param>
        public EventConsumerBase(ITrace tarce)
        {
            Trace = tarce;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="evt"></param>
        public virtual void HandleEvent(EventMessage evt)
        {
            try
            {
                HandleEventInternal(evt);
                UpdateDispatcherInfo();

                string msg = String.Format("处理消息{0}完成", evt.ToString());
                LogInfo(msg);
            }
            catch (System.Exception ex)
            {
                string errmsg = String.Format("处理消息{0}出错,{1}", evt.ToString(), ex.ToString());
                LogError(errmsg);
            }
        }

        
        /// <summary>
        /// 获取日志记录器
        /// </summary>
        /// <returns></returns>
        protected virtual LogManager GetLogger()
        {
            return LogManager.GetLogger("msgconsumer");
        }

        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message"></param>
        protected void LogInfo(string message)
        {
            if (Trace != null)
            {
                Trace.TraceInfo(message);
            }
            GetLogger().LogInfo(message);
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="message"></param>
        protected void LogError(string message)
        {
            if (Trace != null)
            {
                Trace.TraceError(message);
            }

            GetLogger().LogError(message);
        }

        /// <summary>
        /// 更新消息分派信息
        /// </summary>
        protected  void UpdateDispatcherInfo()
        {
            using (var locker = new WriteableLocker(_dispatcherLocker))
            {
                string path = DispatcherDataPath;
                StreamWriter swLog = new StreamWriter(path, false);
                swLog.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                swLog.Flush();
                swLog.Close();
            }
        }

        /// <summary>
        /// 获取分派数据文件路径
        /// </summary>
        /// <param name="consumerName"></param>
        /// <returns></returns>
        protected string DispatcherDataPath
        {
            get
            {
                return String.Format("{0}{1}.dat", SystemEnvironment.RootDirectory, Name.GetHashCode());
            }
        }

        /// <summary>
        /// 最后一次分派消息时间
        /// </summary>
        public DateTime? LastDispatcherTime
        {
            get
            {
                string data = "";

                try
                {
                    string path = DispatcherDataPath;
                    StreamReader swLog = new StreamReader(path);
                    data = swLog.ReadToEnd();
                    swLog.Close();
                }
                catch { }

                DateTime? fetchMsgTime =null;

                if (!String.IsNullOrWhiteSpace(data))
                {
                    fetchMsgTime = Convert.ToDateTime(data) ;
                }

                return fetchMsgTime;
            }
        }

        /// <summary>
        /// 处理消息核心方法
        /// </summary>
        /// <param name="evt"></param>
        protected abstract void HandleEventInternal(EventMessage evt);

        public abstract string Name
        {
            get;
        }

        public abstract List<int> HookMsgClassID
        {
            get;
        }

    }

    
}
