using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.EventBus.Store
{
    public class EmptyMessageStore : IMessageStore
    {
        public bool SaveMessage(EventMessage msg)
        {
            return true;
        }

        public List<EventMessage> GetMessage(List<int> classID, DateTime? from, DateTime? end, int? count)
        {
            return null;
        }
    }
}
