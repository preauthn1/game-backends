using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class PlayerTokenJwtModel
    {       
        public string LoginName { get; set; }

        /// <summary>
        /// 語系
        /// </summary>
        public string Lang { get; set; }

        public long TokenExpiredTime { get; set; }

        public string RndID { get; set; }


        /// <summary>
        /// 加密驗證是否被偽造
        /// </summary>
        public string SignKey { get; set; }


        public int TokenVersion { get; set; }

    }


    public class PlayerJwtGenerateResultModel 
    {

        public string JwtToken { get; set; }


        public PlayerTokenJwtModel JwtModel { get; set; }

    }



}
