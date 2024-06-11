using Hwdtech;

namespace Spaceship__Server;
using System;
using Hwdtech;

public class SoftStopCommand : ICommand
{
    MyThread _thread;
    Action _action;

    public SoftStopCommand(MyThread threadtostop)
    {
        this._thread = threadtostop;
        this._action = () => {};

    }

    public SoftStopCommand(MyThread threadtostop, Action action)
    {
        this._thread = threadtostop;
        this._action = action;
    }

    public Action GetAction(){
        return this._action;
    }

    public void Execute()
    {
        string id = Hwdtech.IoC.Resolve<string>("Get id by thread", _thread);

        new UpdateBehaviourCommand(_thread, () => {
            if(!(_thread.receiver.isEmpty()))
            {  
                _thread.HandleCommand();
            }
            else
            {
                Hwdtech.IoC.Resolve<ICommand>("Send Command", id, Hwdtech.IoC.Resolve<ICommand>("Hard Stop Thread", id, this._action)).Execute();
            }
        }).Execute();
    }
}
