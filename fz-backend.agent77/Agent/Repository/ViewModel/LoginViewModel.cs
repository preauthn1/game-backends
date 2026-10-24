using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        [DisplayName("帳號")]
        public string Account { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [DisplayName("密碼")]
        public string Password { get; set; }       
    }
}
