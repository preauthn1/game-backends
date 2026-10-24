using System;
using System.Collections;
using System.Collections.Generic;
using FW.Core.Engine;


namespace FW.Core.Logger
{
    public class DepencyRegister : SystemDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            engine.Register<ILogFormatter, DefaultLogFormatter>();

            //var parameters = new Dictionary<string, object>();
            //parameters.Add("logFileName", "log");
            //parameters.Add("rateByHour", 12);
            //engine.RegisterWithParameters<ILogger, TxtLoggerByTime>(parameters, "innerLoger");

            //var parameters = new Dictionary<string, object>();
            //parameters.Add("logFileName", "log");
            //parameters.Add("rateBySize", 1024 * 1024 * 5);
            //engine.RegisterWithParameters<ILogger, TxtLoggerBySize>(parameters, "innerLoger");


            //var bufferedLogger = new BufferedLogger(engine.Resolve<ILogger>("innerLoger"), 60);
            //engine.RegisterInstance<ILogger>(bufferedLogger, "bufferedLogger");

            //engine.RegisterInstance<LogManager>(new LogManager(engine.Resolve<ILogger>("bufferedLogger"), engine.Resolve<ILogFormatter>()));

        }
    }
}
