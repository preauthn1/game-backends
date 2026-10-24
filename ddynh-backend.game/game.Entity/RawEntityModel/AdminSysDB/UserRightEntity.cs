using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.RawEntity
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserRightEntity
    {
        [Key]
        public int Id { get; set; }
        public string UserID { get; set; }
        public string RightID { get; set; }
       
    }







}
