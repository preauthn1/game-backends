using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper.Trace
{
    /// <summary>
    /// 控制台
    /// </summary>
    public class ConsoleTrace : ITrace
    {

        public void Trace(string message)
        {
            Console.WriteLine(message);
        }
    }
}
