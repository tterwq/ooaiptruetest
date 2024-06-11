using Hwdtech;
using System;
namespace Spaceship__Server;

public class ActionCommand : ICommand
{
    Action _action;

    public ActionCommand(Action action)
    {
        this._action = action;
    }

    public void Execute()
    {
        this._action();
    }
}
