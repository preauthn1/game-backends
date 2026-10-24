using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class UnionpayModel
    {
        [Required(ErrorMessage = "请输入手机号")]
        public string LoginName { get; set; }

        [Required(ErrorMessage = "请输入銀聯真实姓名")]
        public string UnionpayRealName { get; set; }

        [Required(ErrorMessage = "请输入收款银行")]
        public string UnionpayBank { get; set; }

        [Required(ErrorMessage = "请输入验证码")]
        public string CodeID { get; set; }

        [Required(ErrorMessage = "请输入銀聯账号")]
        public string UnionpayAccount { get; set; }

        [Required(ErrorMessage = "请输入游戏密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        [Compare("Password", ErrorMessage = "密码不一至")]
        public string ConfirmPassword { get; set; }
    }

}
