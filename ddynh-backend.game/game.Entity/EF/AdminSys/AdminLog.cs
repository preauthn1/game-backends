using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.AdminSys
{
    [Database(DBNameConfig.DBName_AdminSys)]
    public class AdminLog : BaseEntity
    {
        public int UserID { get; set; }

        [StringLength(512)]
        public string Memo { get; set; }

        [Required]
        [StringLength(1024)]
        public string Url { get; set; }

        public string Data { get; set; }

        public string IPAddress { get; set; }

        public DateTime WriteTime { get; set; }

        public string Method { get; set; }

        public string RelativeUrl { get; set; }

        public string Memo2 { get; set; }


        public virtual AdminUser User { get; set; }

    }
}
