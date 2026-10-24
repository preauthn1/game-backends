using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class LogonQuestionLog : BaseEntity
    {
        public int UserID { get; set; }
        public int PlatformID { get; set; }
        public string Question { get; set; }
        public QuestionStatus Status { get; set; }
        public byte QuestionType {get; set;}
        public string IPAddress { get; set; }
        public DateTime HandleTime { get; set; }
        public string HandlePerson { get; set; }
        public string ReplyContent { get; set; }
        public DateTime CreateTime { get; set; }
        public string LoginName { get; set; }

    }
}
