namespace Spaceship__Server;

using System.Collections.Concurrent;
using System.Collections.Generic;

public class BCPushCommand : ICommand
{
    BlockingCollection<Spaceship__Server.ICommand> _q;

    List<Spaceship__Server.ICommand> _cmds;

    public BCPushCommand(BlockingCollection<Spaceship__Server.ICommand> q, List<Spaceship__Server.ICommand> cmds)
    {
        this._q = q;
        this._cmds = cmds;
    }

    public void Execute()
    {
        foreach(Spaceship__Server.ICommand cmd in this._cmds)
        {
            _q.Add(cmd);
        }
    }
}
