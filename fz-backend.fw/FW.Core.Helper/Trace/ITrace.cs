using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper.Trace
{
    /// <summary>
    /// 向屏幕输出消息的接口
    /// </summary>
    public interface ITrace
    {
        void Trace(string message);
    }
   
}
