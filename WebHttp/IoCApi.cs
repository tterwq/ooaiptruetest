using System;
using CoreWCF;
using Spaceship__Server;

namespace WebHttp
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class IoCApi : IIoCApi
    {
        public void Init()
        {               
            Dependencies.Run();
        }
    }
}
