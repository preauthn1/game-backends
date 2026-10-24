using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Test
{
    public class InitailHelper
    {
        public static void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "game.Service"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
        }

    }


}
