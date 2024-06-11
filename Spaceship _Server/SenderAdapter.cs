using System.Collections.Concurrent;
using System.Collections.Generic;
using Hwdtech;
namespace Spaceship__Server;

public class SenderAdapter : ISender
{
    public BlockingCollection<Spaceship__Server.ICommand> queue;

    public SenderAdapter(BlockingCollection<Spaceship__Server.ICommand> queue) => this.queue = queue;

    public Spaceship__Server.ICommand Send(object msg)
    {
        Spaceship__Server.ICommand cmd = IoC.Resolve<Spaceship__Server.ICommand>("Message deserialize", msg);
        
        BCPushCommand pusher = new(queue, new List<Spaceship__Server.ICommand>(){cmd});

        return pusher;
    }
}
