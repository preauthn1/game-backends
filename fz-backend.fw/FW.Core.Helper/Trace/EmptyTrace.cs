using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper.Trace
{
    /// <summary>
    /// 空的Trace实现
    /// </summary>
    public class EmptyTrace : ITrace
    {
        public void Trace(string message)
        {

        }
    }
}
