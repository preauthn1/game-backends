using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.Utility.IOC;
using target.Utility.Utility.ThirdParty;
using target.Utility.Extensions;

namespace target.API.Job.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        public BaseController()
        {
            InstanceAllServiceImplement(this);
        }


        protected void InstanceAllServiceImplement<T>(T controller) where T : class
        {
            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.PropertyType.IsInterface)
                .Where(x => x.Name.ToLower().Contains("service"))
                .Where(x => !x.Name.ToLower().Contains("integrationservice")) //遊戲商service不採用自動注入
                .ToList();

            foreach (var item in allService)
            {
                var instanceValue = IOCHelper.Resolve(item.PropertyType);
                item.SetValue(this, instanceValue);
            }
        }



        protected void AsyncAndTryCatch(Action action)
        {
            StackTrace stackTrace = new StackTrace();
            string previousMethodName = stackTrace.GetFrame(1)?.GetMethod()?.Name;
            string apiPath = $" `{HttpContext?.Request?.Path}` ";

            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    string prefix = $" `{apiPath}` {previousMethodName} ";

                    string msg = "AsyncAndTryCatch錯誤：" +  ex.GetCommonNotifyMessage(prefix);

                    bool isIgnoreNotify = apiPath.ContainsWihtoutCase("exceptional-wagers");

                    if (!isIgnoreNotify)
                    {
                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            TelegramChannelEnum = TelegramChannelEnum.pgs_job_api,
                            Message = msg
                        });

                        MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                        {
                            Channel = SlackChannelEnum.n_pgs_job_api,
                            Message = msg,
                        });
                    }
                }
            });
        }

    }
}
