using System;
using System.ComponentModel.DataAnnotations;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class Organization 
    {

        [Key]
        public int OrgID { get; set; }

        public string Name { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }


    }
}
