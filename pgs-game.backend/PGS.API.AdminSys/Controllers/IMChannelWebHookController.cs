using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.Enums;
using target.Models;
using target.Models.Models;
using target.BL.Services;
using target.Utility;
using target.Utility.Extensions;
using TG_Hook_API.Models;
using target.Utility.Utility.ThirdParty;
using target.Utility.Helper;

namespace target.API.AdminSys.Controllers
{
    public class IMChannelWebHookController : BaseController
    {
        public IAdminSysTGBotHookComplexService AdminSysTGBotHookComplexService { get; set; }


        [Route("TG_BOT_HOOK/pgs_it_bot")]
        [HttpPost]
        public object HookCallback_TG([FromBody] object myModel)
        {
            TgWebhookParsingResult parse = ParsingJsonToModel(myModel);

            if (parse.ParsingModel == null)
                return "ok";

            if (parse.ParsingModel.message == null)
                return "ok";

            var bot_secret = [REDACTED]"X-Telegram-Bot-Api-Secret-Token");

            if(bot_secret != SettingHelper.ThirdParty.Telegram_Bot_SecretToken)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    ColumnName = "Telegram_Bot_SecretToken",
                });
            }

            AdminSysTGBotHookComplexService.CheckWebHookActionAndExecute_TG(parse);
            return "ok";
        }


        [Route("Skype_BOT_HOOK/pgs_it_bot")]
        [HttpPost]
        public object HookCallback_Skype([FromBody] SkypeWebHookRequest myModel)
        {
            //skype API已死 不再支援
            //AdminSysTGBotHookComplexService.CheckWebHookActionAndExecute_Skype(myModel);


            return "ok";
        }



        private TgWebhookParsingResult ParsingJsonToModel(object model)
        {
            string rawJson = string.Empty;

            try
            {
                rawJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                var convert = Newtonsoft.Json.JsonConvert.DeserializeObject<TgWebhookRequest>(rawJson);

                return new TgWebhookParsingResult
                {
                    RawJson = rawJson,
                    ParsingModel = convert
                };
            }
            catch
            {


            }

            return new TgWebhookParsingResult
            {
                RawJson = rawJson,
                ParsingModel = null
            };
        }

    }
}
