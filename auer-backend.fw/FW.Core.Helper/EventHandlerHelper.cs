using System;


namespace FW.Core.Helper
{
    public static class EventHandlerHelper
    {
        /// <summary>
        /// 异步触发事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void AsyncInvoke<TEventArgs>(this EventHandler<TEventArgs> handler, object sender, TEventArgs args)
        {
            if(handler == null)
            {
                return;
            }

            var evts = handler.GetInvocationList();
            foreach (var evt in evts)
            {
                (evt as EventHandler<TEventArgs>).BeginInvoke(sender, args, null, null);
            }
        }

        /// <summary>
        /// 异步触发事件
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void AsyncInvoke(this EventHandler handler,EventArgs args, object sender)
        {
            if (handler == null)
            {
                return;
            }

            var evts = handler.GetInvocationList();
            foreach (var evt in evts)
            {
                (evt as EventHandler).BeginInvoke(sender, args, null, null);
            }
        }

        /// <summary>
        /// 同步触发事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="handler"></param>
        /// <param name="arg"></param>
        /// <param name="sender"></param>
        public static void SyncInvoke<TEventArgs>(this EventHandler<TEventArgs> handler, TEventArgs arg, object sender)
        {
            if (handler == null)
            {
                return;
            }

            var evts = handler.GetInvocationList();
            foreach (var evt in evts)
            {
                (evt as EventHandler<TEventArgs>).Invoke(sender, arg);
            }

        }

        /// <summary>
        /// 同步触发事件
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="?"></param>
        /// <param name="args"></param>
        /// <param name="sender"></param>
        public static void SyncInvoke(this EventHandler handler, EventArgs args, object sender)
        {
            if (handler == null)
            {
                return;
            }

            var evts = handler.GetInvocationList();
            foreach (var evt in evts)
            {
                (evt as EventHandler).Invoke(sender, args);
            }
        }

    }
}
