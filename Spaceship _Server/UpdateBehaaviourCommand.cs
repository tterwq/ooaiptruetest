using System;

namespace Spaceship__Server;

public class UpdateBehaviourCommand : Spaceship__Server.ICommand
{
    Action behaviour;
    MyThread thread;

    public UpdateBehaviourCommand(MyThread thread, Action newBehaviour)
    {
        this.behaviour = newBehaviour;
        this.thread = thread;
    }
    public void Execute()
    {
        thread.UpdateBehaviour(this.behaviour);
    }
}
