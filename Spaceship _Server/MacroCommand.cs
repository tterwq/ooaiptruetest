using System.Collections.Generic;
using Hwdtech;
namespace Spaceship__Server;

public class MacroCommand : ICommand
{
    Queue<Spaceship__Server.ICommand> _queue = new();
    List<Spaceship__Server.ICommand> _jobs{get; set;}

    public MacroCommand(Queue<Spaceship__Server.ICommand> queue, List<Spaceship__Server.ICommand> jobs)
    {
        this._queue = queue;
        this._jobs = jobs;
    }

    public void Execute()
    {
        foreach(Spaceship__Server.ICommand job in _jobs)
        {
            _queue.Enqueue(job);
        }
    }

    
}
