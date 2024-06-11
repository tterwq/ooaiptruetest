namespace Spaceship__Server;

public class HardStopCommand : ICommand
{
    MyThread thread;
    public HardStopCommand(MyThread threadtostop)
    {
        this.thread = threadtostop;
    }

    public void Execute()
    {
        new ThreadStopCommand(thread).Execute();
    }
}
