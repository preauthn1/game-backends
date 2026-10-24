using FW.Core.Engine;
using System;


namespace FW.Core.EventBus
{
    public class DepencyRegister : SystemDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            var bus = new EmptyEventBus();

            engine.RegisterInstance<IEventBus>(bus);
            engine.RegisterInstance<IEventPublisher>(bus);
        }
    }
}
