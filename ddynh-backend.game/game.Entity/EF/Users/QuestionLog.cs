using System;
using System.ComponentModel.DataAnnotations.Schema;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class QuestionLog : BaseEntity
    {
        public int UserID { get; set; }
        public string Question { get; set; }
        public QuestionStatus Status { get; set; }
        public byte QuestionType { get; set; }
        public string IPAddress { get; set; }
        public DateTime HandleTime { get; set; }
        public string HandlePerson { get; set; }
        [NotMapped]
        public string DisplayName { get; set; }
        public string ReplyContent { get; set; }
        public DateTime CreateTime { get; set; }

        public string UploadImage { get; set; }

     
        public virtual UserInfo User
        {
            get;
            set;
        }

    }
   
}
