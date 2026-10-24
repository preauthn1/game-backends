using FW.Core.Helper;
using game.Entity.Notify;
using System;
using System.Collections.Generic;

namespace game.Service
{
    public interface INotifyService
    {
        List<SMSVCode> GetSMS(int codeID);
        void UpdateSMSSendResult(int codeID, int status, string sendResult);
        List<SMSVender> GetSMSVender();
        void EnableSMSVender(byte id);
        PagedList<SMSLog> GetSMSLog(int operatorID, string tel, int? UserID, DateTime? BeginTime, DateTime? EndTime, int pageIndex, int pageSize);
    }
}
