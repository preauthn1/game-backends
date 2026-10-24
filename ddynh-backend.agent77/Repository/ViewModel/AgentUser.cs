using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class AgentUser
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "请输入账号")]        
        public string Account { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "密码至少6位数")]
        [Compare("Password", ErrorMessage = "新密码不一至")]
        public string ConfirmPassword { get; set; }

        public string Encrypt { get; set; }

        [Required(ErrorMessage = "请选择身份")]
        public int? AdminType { get; set; }
       
        public string PlatformID { get; set; }

        public bool Mode1 { get; set; }

        public bool Mode2 { get; set; }

        public bool Mode3 { get; set; }

        public bool Mode4 { get; set; }

        public bool Mode5 { get; set; }

        public bool Mode6 { get; set; }
    }
}
