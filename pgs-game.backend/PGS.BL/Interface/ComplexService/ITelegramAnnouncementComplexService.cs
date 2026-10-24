using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface ITelegramAnnouncementComplexService
    {

        TelegramAnnouncementSubmitViewModel GetDefaultContentByTemplateID(int userID, LazyCommonRequestModel model);

        CRUDResultModel SendTGNotifyByAPI(int userID, TelegramAnnouncementSubmitViewModel model);

        void SendLog(TelegramAnnouncementSendLogRequestModel model);

        List<IMChannelAnnouncementHistoryViewModel> GetNotifyHistory(int userID, LazyCommonRequestModel model);

        void RemoveSingleMessage(int userID, LazyCommonRequestModel model);

        void RemoveLatestMessages(int userID, LazyCommonRequestModel model);

        void SycnTGBotAndChatID();

        string ReplacePGSKeywordToIONSLOT(string skypeNotifyMsg);
    }
}
