using Lck.Utility.Extensions.Helper;

using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility.Extensions;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace target.Utility.Helper
{

    /// <summary>
    /// 雖然號稱queue 但是懶得建置服務 (MQ)
    /// </summary>
    public class BaseQueueHelper<T> where T: class
    {
        private const int MAX_SIZE = 2500;

        protected static ConcurrentDictionary<string, Channel<T>> ConcurrentQueue = new ConcurrentDictionary<string, Channel<T>>();

        protected string MyIdentityKey{ get; set; }

        protected Channel<T> MyQueue
        {
            get
            {
                return ConcurrentQueue[MyIdentityKey];
            }
        }


        public BaseQueueHelper(string identityKey)
        {
            this.MyIdentityKey = identityKey;

            try
            {
                var instance = Channel.CreateBounded<T>(
                   new BoundedChannelOptions(MAX_SIZE)
                   {
                       SingleReader = true,
                       SingleWriter = false,
                       FullMode = BoundedChannelFullMode.DropWrite
                   });

                ConcurrentQueue.TryAdd(MyIdentityKey, instance);
            }
            catch(Exception ex)
            {
                string debug = "";
            }             
        }


        public void Append(T data)
        {
            if (data == null)
                return;

            AppendMultiple(new List<T> { data });
        }


        public void AppendMultiple(List<T> items)
        {
            try
            {
                foreach (var item in items)
                {
                    MyQueue.Writer.TryWrite(item);
                }
            }
            catch(Exception ex)
            {

            }            
        }

        public List<T> GetMultipleItems(int takeSize = 200)
        {
            List<T> listResult = new List<T>();

            for (int i = 0; i < takeSize; i++)
            {
                T myModel = null;

                var isDequeue = MyQueue.Reader.TryRead(out myModel);

                if (!isDequeue || myModel == null)
                {
                    break;
                }

                listResult.Add(myModel);
            }

            return listResult;
        }


        private void SendMaxSizeWarning()
        {
            string myMsg = $"{RuntimeHelper.GetK8sInstanceName()}、{MyIdentityKey} 已出現queue size達到上限。可能已經處理不來了";

            string myCacheKey = "SendMaxSizeWarning_" + MyIdentityKey;

            if (!CacheHelper.CheckShortCacheLockerExist(myCacheKey, 60 * 30, isThrowEx: false))
            {
                MyTelegramUtility.SendMessage(new TelegramMessageParameter
                {
                    Message = myMsg,
                    TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                });
            }
        }

    }


}
