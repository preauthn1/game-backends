using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class UserPermissionMappingEntity
    {
        [Key]
        public int Id { get; set; }

        public int UserID { get; set; }

        public int PermissionID { get; set; }

    }
}
