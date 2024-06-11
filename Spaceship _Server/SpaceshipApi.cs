using System;
using CoreWCF;
using System.Collections.Generic;
using System.Collections;


namespace Spaceship__Server
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class SpaceshipApi : ISpaceshipApi
    {
        public JSONContract Message(JSONContract req)
        {               
            Dictionary<string, object> content = (Dictionary<string, object>)req.Value.Entries;

            Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Deserialize Message to Command", content).Execute();

           return req;
        }

        public void Init()
        {               
            Dependencies.Run();

            Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2");
        }
    }
}
