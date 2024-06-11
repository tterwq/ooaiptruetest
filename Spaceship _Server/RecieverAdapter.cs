using System;
using System.Collections.Concurrent;


namespace Spaceship__Server;


public class RecieverAdapter : IReciver
{
    public BlockingCollection<Spaceship__Server.ICommand> queue;

    public RecieverAdapter(BlockingCollection<Spaceship__Server.ICommand> queue) => this.queue = queue;

    public Spaceship__Server.ICommand Receive()
    {
        return queue.Take();
    }

    public bool isEmpty()
    {
        return (queue.Count == 0);
    }
}
