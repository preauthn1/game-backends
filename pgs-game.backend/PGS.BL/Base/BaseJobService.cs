using target.Core.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.DA.Repository;
using target.Models.Table;
using target.Utility.Helper;
using target.Models.ViewModels.Base;
using Lck.Utility.Extensions.Helper;
using target.Utility.IOC;
using target.Utility;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{
    public class BaseJobService : BaseService
    {

        protected static int _defaultUserID = 0;


        protected void SetSystemDefaultUserID()
        {
            if (_defaultUserID <= 0)
            {
                var UserRepository = new UserRepository();

                string targetUserName = SettingHelper.DefaultUserName;

                var queryDefaultUser = UserRepository.GetByAccount(targetUserName);

                if (queryDefaultUser == null)
                {
                    string msg = $"查無user: {targetUserName} config可能漏配置";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_job_api);

                    throw new TerproException(ApiErrorCode.NotFoundUser, new ErrorDetailModel
                    {
                        LogExtraInfo = $"{targetUserName}",
                    });
                }

                _defaultUserID = queryDefaultUser.UserID;
            }
        }



        protected void NotifyToMultipleJobChannel(string msg , string ignoreKeyword = "")
        {
            if (SettingHelper.IsMSTest)
                return;

            //如果有指定特定關鍵字 比對到就不發通知了 (e.g. 一些預期知道會偶發的常態錯誤)
            if (!string.IsNullOrWhiteSpace(ignoreKeyword))
            {
                if(msg.Contains(ignoreKeyword))
                {
                    return;
                }
            }

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                TelegramChannelEnum = TelegramChannelEnum.pgs_job_api,
                Message = msg
            });

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_job_api,
                Message = msg
            });

        }


    }
}
