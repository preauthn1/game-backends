using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class AdminLog
    {
        public int Id { get; set; }

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


        public virtual AdminUser User
        {
            get;
            set;
        }

    }
}
