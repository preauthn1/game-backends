using game.Entity.Event;
using game.Entity.Users;
using game.Infrastructure.Extensions;

namespace game.AdminSystem.Models.ViewModels
{
    public class EventDetailLogViewModels : EventDetailLog
    {
        public string EventTypeName
        {
            get
            {
                return ((ApplyEventTypeEnum)this.EventType).GetEnumDescription();
            }
        }

    }






}