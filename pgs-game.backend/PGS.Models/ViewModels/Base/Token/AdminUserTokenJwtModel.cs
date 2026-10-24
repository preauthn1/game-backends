using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class AdminUserTokenJwtModel : ITokenJwtModel
    {
        public int LoginUserID { get; set; }


        public string LoginName { get; set; }
        

        public int OrganizationID { get; set; }

        /// <summary>
        /// 存timestamp
        /// </summary>
        public long TokenExpiredTime { get; set; }


        public string RndID { get; set; }

        /// <summary>
        /// 加密驗證是否被偽造
        /// </summary>
        public string SignKey { get; set; }


        public int TokenVersion { get; set; }


        public int JwtTokenType { get; set; }
    }
}
