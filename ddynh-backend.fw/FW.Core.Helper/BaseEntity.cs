using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper
{
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Key]
        public int Id { get; set; }
    }
}
