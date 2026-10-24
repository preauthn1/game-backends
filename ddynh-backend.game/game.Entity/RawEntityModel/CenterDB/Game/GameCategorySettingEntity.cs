using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Game
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GameCategorySettingEntity
    {
        [Key]
        public int Id { get; set; }

        public int GameID { get; set; }

        public int GameCategoryID { get; set; }

    }

}
