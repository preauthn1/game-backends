using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class ModifyPWD
    {
        [Required(ErrorMessage = "请输入旧密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "请输入新密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入新密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        [Compare("Password", ErrorMessage = "新密码不一至")]
        public string ConfirmPassword { get; set; }
    }
}
