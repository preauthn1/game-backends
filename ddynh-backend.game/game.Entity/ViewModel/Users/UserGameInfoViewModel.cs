using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    
    public class UserGameInfoViewModel : UserGameInfoEntity
    {       

        public string GameName { get; set; }


        /// <summary>
        /// 需透過另一個查詢補齊這邊的資料
        /// </summary>
        public UserInfo UserInfo { get; set; }
        
    }
}
