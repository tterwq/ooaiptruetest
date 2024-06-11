using System;
using System.Threading;
using Hwdtech;
namespace Spaceship__Server;

public class MyThread
{
    public Thread thread;
    public IReciver receiver;
    public bool stop = false;
    public Action strategy;

    internal void Stop() => stop = true;

    internal void HandleCommand()
    {
        var cmd = receiver.Receive();

        cmd.Execute();
    }
    public MyThread(IReciver queue)
    {
        this.receiver = queue;
        strategy = () =>
        {
            HandleCommand();
        };

        thread = new Thread(() =>
        {
            while (!stop) strategy();
        });
    }
    internal void UpdateBehaviour(Action newBehaviour)
    {
        strategy = newBehaviour;

    }
    public void Start()
    {
        thread.Start();
    }
}
