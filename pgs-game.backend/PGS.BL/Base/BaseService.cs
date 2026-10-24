using target.Core.Repository;
using target.Models.Paging;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Helper;
using target.Utility;
using target.Models.CoreLibs;

namespace target.BL.Services
{
    public class BaseService
    {
        public static RedisCacheInstance RedisCache_1 = new RedisCacheInstance(CacheInstanceCategory.Redis);

        public static RedisCacheInstance RedisCache_2 = new RedisCacheInstance(CacheInstanceCategory.Redis002); //比較雜亂、注單型的cache會存在這個

        public static RedisCacheInstance RedisCache_3 = new RedisCacheInstance(CacheInstanceCategory.Redis003); //比較雜亂、注單型的cache會存在這個

        public virtual bool IsAutoInstanceAllRepository
        {
            get
            {
                return true;
            }
        }

        public virtual bool IsAutoInstanceAllService
        {
            get
            {
                return true;
            }
        }

        public BaseService()
        {
            InstanceAllRepositoryAndService(this.IsAutoInstanceAllRepository, this.IsAutoInstanceAllService);
        }


        public void InstanceAllRepositoryAndService(bool isInstanceRep, bool isInstanceService)
        {
            //如果service class內的全域變數
            //1. 定義為 Property (非Field)
            //2. 且命名包含 repository
            //3. 有繼承 IDapperRepository

            if (isInstanceRep)
            {
                this.InstanceAllRepository();
            }

            if (isInstanceService)
            {
                this.InstanceAllService();
            }
        }


        protected void InstanceAllRepository()
        {
            var allRep = GetType()
               .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
               .Where(x => x.Name.ToLower().Contains("repository") || x.Name.ToLower().Contains("rep"))
               .ToList();

            foreach (PropertyInfo item in allRep)
            {
                if (item.PropertyType.BaseType != null)
                {
                    if (item.PropertyType.BaseType.IsGenericType &&
                        (
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatIntegerPrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatLongPrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatStringPrimaryKeyRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatBetRecordRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(BaseMongoRepository<>) ||
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(FormatMultiplePrimaryKeyRepository<>)
                        ))
                    {
                        var instanceValue = Activator.CreateInstance(item.PropertyType);
                        item.SetValue(this, instanceValue);
                    }
                }
            }
        }


        protected void InstanceAllService()
        {
            var allService = GetType()
                  .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                  .Where(x => x.Name.ToLower().Contains("service"))
                  .ToList();

            foreach (PropertyInfo item in allService)
            {
                if (item.PropertyType.IsInterface)
                {
                    var doNotInjectAttr = item.GetTargetAttribute<DoNotAutoInject>();

                    if (doNotInjectAttr == null)
                    {
                        var instanceValue = IOCHelper.Resolve(item.PropertyType);
                        item.SetValue(this, instanceValue);
                    }
                    else
                    {
                        string debug = "";
                    }
                }

            }
        }


        protected T InstanceRepository<T>()
        {
            return (T)Activator.CreateInstance(typeof(T));
        }


        /// <summary>
        /// 轉換成另一種class的 PagedList 物件
        /// </summary>
        protected PagedList<T_New> ConvertToNewTypePagingObject<T_Original, T_New>(PagedList<T_Original> model, bool isAppendList = false) where T_New : new()
        {
            var resulk = model.ReturnInjecteObject<PagedList<T_New>>();

            if (isAppendList && model.Items.IsNotEmptyNull())
            {
                resulk.Items = model.Items.Select(x => x.ReturnInjecteObject<T_New>()).ToList();
            }

            return resulk;
        }


        public virtual void SendMessageToMultipleChannel(string msg, TelegramChannelEnum tgChannel = TelegramChannelEnum.pgs_common_error)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            var slackChannel = SlackChannelEnum.n_error_pgs_common;

            //TG與slack頻道對應
            switch (tgChannel)
            {
                case TelegramChannelEnum.pgs_externalAPI:
                    slackChannel = SlackChannelEnum.n_error_external_api;
                    break;

                case TelegramChannelEnum.pgs_job_api:
                    slackChannel = SlackChannelEnum.n_pgs_job_api;
                    break;

                case TelegramChannelEnum.pgs_single_wallet_erorr:
                    slackChannel = SlackChannelEnum.n_error_merchant_api;
                    break;

                case TelegramChannelEnum.pgs_adminSys_api:
                    slackChannel = SlackChannelEnum.n_error_adminsys;
                    break;

                case TelegramChannelEnum.pgs_important_error:
                    slackChannel = SlackChannelEnum.n_error_important;
                    break;

                default:
                    break;
            }

            //字數不多的話順便印出instance name
            if (msg.Count() <= 100)
            {
                string ks8Name = RuntimeHelper.GetK8sInstanceName();
                msg = $" `{ks8Name}` " + msg;
            }

            SendMessageToSlack(msg, slackChannel);

            SendMessageToTelegram(msg, tgChannel);
        }

        protected void SendMessageToSlack(string msg, SlackChannelEnum channel = SlackChannelEnum.n_error_pgs_common)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = channel
            });
        }

        protected void SendMessageToTelegram(string msg, TelegramChannelEnum tgChannel = TelegramChannelEnum.pgs_common_error)
        {
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = tgChannel
            });
        }

    }
}
