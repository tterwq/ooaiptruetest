using System;
using System.Threading;

namespace Spaceship__Server;

public class ThreadStopCommand : Spaceship__Server.ICommand
{
    MyThread stoppingThread;
    public ThreadStopCommand(MyThread stoppingThread) => this.stoppingThread = stoppingThread;

    public void Execute()
    {
        if (Thread.CurrentThread == stoppingThread.thread)
        {
            stoppingThread.Stop();
        }
        else
        {
            throw new Exception();
        }
    }
}
