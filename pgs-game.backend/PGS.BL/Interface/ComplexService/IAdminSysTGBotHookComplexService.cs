using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using TG_Hook_API.Models;

namespace target.BL.Interface
{
    public interface IAdminSysTGBotHookComplexService
    {
        void CheckWebHookActionAndExecute_TG(TgWebhookParsingResult parse);

        void CheckWebHookActionAndExecute_Skype(SkypeWebHookRequest parse);

        void RenameChannelName(TgRenameChannelTitleParameter model);

        void CreateNewChannelTG(TgNewChannelJoinParameter model);
    }

}
