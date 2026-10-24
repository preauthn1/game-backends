using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IMessageService
    {
        MessageViewModel GetPlayerMessages(int playerID);

        void SetMessageHintRead(int playerID, int type);

        void SetMessageRead(int playerID, MessageReadRequestModel model);
    }
}
