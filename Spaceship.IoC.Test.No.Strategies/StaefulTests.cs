namespace Spaceship.IoC.Test.No.Strategies;
using System.Collections.Concurrent;
using Moq;
using Hwdtech;
using Spaceship__Server;
using System.Threading;

public class Stateful
{
    [Fact]
    public object CreateIoCDependencies()
    {
        new Hwdtech.Ioc.InitScopeBasedIoCImplementationCommand().Execute();

        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        Dictionary<string, MyThread> GameThreads = new();

        Dictionary<string, ISender> GameSenders = new();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get id by thread", (object[] args) => {
            MyThread thread = (MyThread)args[0];

            return GameThreads.FirstOrDefault(t => t.Value == thread).Key;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get sender by id", (object[] args) => 
        {
            string id = (string)args[0];
            
            return GameSenders[id];
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get thread by id", (object[] args) => 
        {
            string id = (string)args[0];
            
            return GameThreads[id];
        }).Execute();
        
        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create and Start Thread", (object[] args) => 
        {
            if (args.Count() == 2)
            {
                string id = (string)args[0]; 
                Action action = (Action)args[1];
                BlockingCollection<Spaceship__Server.ICommand> q = new();

                ISender sender = new SenderAdapter(q);
                IReciver receiver = new RecieverAdapter(q);
                MyThread thread = new(receiver);

                q.Add(new ActionCommand(action));

                thread.Start();

                GameThreads.Add(id, thread);
                GameSenders.Add(id, sender);

                return thread;
            }
            else{
                string id = (string)args[0]; 
                BlockingCollection<Spaceship__Server.ICommand> q = new();

                ISender sender = new SenderAdapter(q);
                IReciver receiver = new RecieverAdapter(q);
                MyThread thread = new(receiver);

                thread.Start();

                GameThreads.Add(id, thread);
                GameSenders.Add(id, sender);

                return thread;
            }
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Send Command", (object[] args) => 
        {
            string id = (string)args[0]; 

            ISender sender = IoC.Resolve<ISender>("Get sender by id", id);

            Spaceship__Server.ICommand cmd = (Spaceship__Server.ICommand)args[1];

            return sender.Send(cmd);
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Message deserialize", (object[] args) => {
            Spaceship__Server.ICommand cmd = (Spaceship__Server.ICommand) args[0];

            return cmd;
        }).Execute();     

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Hard Stop Thread", (object[] args) => 
        {
            ISender sender = IoC.Resolve<ISender>("Get sender by id", (string)args[0]);
            if (args.Count() == 2)
            {
                string id = (string)args[0]; 
                Action action = (Action)args[1];

                MyThread thread = IoC.Resolve<MyThread>("Get thread by id", id);
                Action act = thread.strategy + action;
                BCPushCommand send = new BCPushCommand(((SenderAdapter)sender).queue, new List<Spaceship__Server.ICommand>(){
                    new UpdateBehaviourCommand(thread, act),
                    new HardStopCommand(IoC.Resolve<MyThread>("Get thread by id", id))});

                return send;
            }
            else{
                string id = (string)args[0]; 

                MyThread thread = IoC.Resolve<MyThread>("Get thread by id", id);
                BCPushCommand send = new BCPushCommand(((SenderAdapter)sender).queue, new List<Spaceship__Server.ICommand>(){
                    new HardStopCommand(IoC.Resolve<MyThread>("Get thread by id", id))});

                return send;
            }
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Soft Stop Thread", (object[] args) => 
        {
            ISender sender = IoC.Resolve<ISender>("Get sender by id", (string)args[0]);
            if (args.Count() == 2)
            {
                string id = (string)args[0];
                Action action = (Action)args[1];
                MyThread thread = IoC.Resolve<MyThread>("Get thread by id", id);

                SoftStopCommand cmd = new SoftStopCommand(thread, action);

                BCPushCommand send = new BCPushCommand(((SenderAdapter)sender).queue, new List<Spaceship__Server.ICommand>(){cmd});

                return send;
            }
            else{
                string id = (string)args[0];
                MyThread thread = IoC.Resolve<MyThread>("Get thread by id", id);

                SoftStopCommand cmd = new SoftStopCommand(thread);

                BCPushCommand send = new BCPushCommand(((SenderAdapter)sender).queue, new List<Spaceship__Server.ICommand>(){cmd});

                return send;
            }
        }).Execute();

        return scope;
    }

    [Fact]
    public void WrongThreadStop()
    {
        Dependencies.Run();
        BlockingCollection<Spaceship__Server.ICommand> q = new();
        BlockingCollection<Spaceship__Server.ICommand> q1 = new();

        AutoResetEvent waiter = new(false);

        IReciver receiver = new RecieverAdapter(q);
        IReciver receiver1 = new RecieverAdapter(q);
        MyThread thread = new(receiver);
        MyThread wrongthread = new(receiver1);

        Action action = () => {
            Assert.Throws<Exception>(() => {
            waiter.Set();
            new HardStopCommand(wrongthread).Execute();
            });
        };

        q.Add(new ActionCommand(action));

        thread.Start();

        waiter.WaitOne();
    }
    
    [Fact]
    public void RecieverAdapterTests()
    {
        BlockingCollection<Spaceship__Server.ICommand> q = new();

        Mock<Spaceship__Server.ICommand> cmd = new();

        q.Add(cmd.Object);

        IReciver rec = new RecieverAdapter(q);

        Assert.Equal(cmd.Object, rec.Receive());

        Assert.True(rec.isEmpty());
    }

    [Fact]
    public void SenderAdapterTests()
    {
        BlockingCollection<Spaceship__Server.ICommand> q = new();

        Mock<Spaceship__Server.ICommand> cmd = new();

        ISender rec = new SenderAdapter(q);

        Assert.Empty(q);

        rec.Send(cmd.Object).Execute();

        Assert.Single(q);

    }

    [Fact]
    public void AdaptersFieldsTest()
    {
        BlockingCollection<Spaceship__Server.ICommand> q = new();

        RecieverAdapter rec = new RecieverAdapter(q);

        SenderAdapter snd = new SenderAdapter(q);

        Assert.Equal(q, snd.queue);

        Assert.Equal(q, rec.queue);
    }

    [Fact]
    public void SendSingleCommandIntoLambdaInitializedThread()
    {
        Dependencies.Run();

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1", () => {});

        AutoResetEvent waiter = new(false);

        ActionCommand cmd =  new(() => {Assert.Single(((RecieverAdapter)thread.receiver).queue);});

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        cmd =  new(() => {waiter.Set();});

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        waiter.WaitOne();

        Assert.Empty(((RecieverAdapter)thread.receiver).queue);
    }

    [Fact]
    public void SendSingleCommandIntoLambdaLessInitializedThread()
    {
        Dependencies.Run();

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1");

        AutoResetEvent waiter = new(false);

        ActionCommand cmd =  new(() => {Assert.Single(((RecieverAdapter)thread.receiver).queue);});

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        cmd =  new(() => {waiter.Set();});

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        waiter.WaitOne();

        Assert.Empty(((RecieverAdapter)thread.receiver).queue);
    }
    
    [Fact]
    public void SoftStopThread()
    {
        object scope = Dependencies.Run();


        AutoResetEvent waiter = new(false);

        ActionCommand cmd = new(() => {});

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1", () => {IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();});
        MyThread thread2 = IoC.Resolve<MyThread>("Create and Start Thread", "3", () => {IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();});

        IoC.Resolve<Spaceship__Server.ICommand>("Soft Stop Thread", "3").Execute();
        IoC.Resolve<Spaceship__Server.ICommand>("Soft Stop Thread", "1", () => {waiter.Set();}).Execute();        

        Assert.False(thread.stop);

        waiter.WaitOne();

        Assert.True(thread.stop);
    }
    

    [Fact]
    public void SoftStopAction()
    {
        BlockingCollection<Spaceship__Server.ICommand> q = new();

        IReciver ra = new RecieverAdapter(q);

        MyThread thread = new(ra);

        SoftStopCommand ssc = new(thread);

        SoftStopCommand ssc2 = new(thread);

        Assert.Equal(ssc2.GetAction(), ssc.GetAction());
    }
    [Fact]
    public void HardStopThread()
    {
        Dependencies.Run();

        AutoResetEvent waiter = new(false);

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1");
        MyThread thread2 = IoC.Resolve<MyThread>("Create and Start Thread", "3");


        Assert.False(thread.stop);
        Assert.False(thread2.stop);

        IoC.Resolve<Spaceship__Server.ICommand>("Hard Stop Thread", "1").Execute();
        IoC.Resolve<Spaceship__Server.ICommand>("Hard Stop Thread", "3", () => {waiter.Set();}).Execute();

        waiter.WaitOne();

        Assert.True(thread.stop);
        Assert.True(thread2.stop);
    }
    
    [Fact (Skip = "Bad test")]
    public void SoftAwaitTest()
    {
        var scope = Dependencies.Run();

        AutoResetEvent waiter = new(false);

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1", () => {IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();});

        ActionCommand cmd = new(() => {});

        Assert.True(thread.receiver.isEmpty());

        IoC.Resolve<Spaceship__Server.ICommand>("Soft Stop Thread", "1", () => {waiter.Set();}).Execute();

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        waiter.WaitOne();

        Assert.True(thread.receiver.isEmpty());
    }
    
    [Fact (Skip = "Bad test")]
    public void HardNonAwaitTest()
    {
        var scope = Dependencies.Run();

        AutoResetEvent waiter = new(false);

        MyThread thread = IoC.Resolve<MyThread>("Create and Start Thread", "1", () => {IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();});

        ActionCommand cmd = new(() => {});

        Assert.True(thread.receiver.isEmpty());

        IoC.Resolve<Spaceship__Server.ICommand>("Hard Stop Thread", "1", () => {waiter.Set();}).Execute();

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "1", cmd).Execute();

        waiter.WaitOne();

        Assert.False(thread.receiver.isEmpty());
    }
}
