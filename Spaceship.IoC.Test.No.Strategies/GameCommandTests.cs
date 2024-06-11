using Hwdtech;
using System;
using Spaceship__Server;
using System.Collections.Concurrent;
using System.Diagnostics;
using Moq;

namespace Spaceship.IoC.Test.No.Strategies;


public class GameCommandTests
{
    [Fact]
    public object Init()
    {
        new Hwdtech.Ioc.InitScopeBasedIoCImplementationCommand().Execute();

        var scope = Hwdtech.IoC.Resolve<object>("Scopes.New", Hwdtech.IoC.Resolve<object>("Scopes.Root"));

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        TimeSpan ts = new TimeSpan(0, 0, 0, 0, 100);

        bool exceptionWasHandled = false;

        bool commandExecuted = false;

        Mock<Spaceship__Server.ICommand> mcmd = new();

        mcmd.Setup(c => c.Execute()).Callback(() => {commandExecuted = true;});

        Spaceship__Server.ICommand cmd = mcmd.Object;

        Mock<Spaceship__Server.ICommand> mcmd2 = new();

        mcmd2.Setup(c => c.Execute()).Callback(() => {(new ExceptionThrower()).ThrowEx();});

        Spaceship__Server.ICommand cmd2 = mcmd2.Object;

        Queue<Spaceship__Server.ICommand> queue = new(new[] {cmd2, cmd, cmd, cmd});

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Exception.Source", (object[] args) => 
        {
        
            Exception ex = (Exception)args[0];
            var a = (new StackTrace(ex).GetFrame(0)!.GetMethod()!.ReflectedType)!.FullName;
            return a;
            
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Exception.Status", (object[] args) => 
        {
            return (object)exceptionWasHandled;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get.Command.Status", (object[] args) => 
        {
            return (object)commandExecuted;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register" , "Game.Current.Timespan", (object[] args) => 
        {
            return (object) ts;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register" , "Game.Current.Set.Timespan", (object[] args) => 
        {
            return new ActionCommand(() => {
                TimeSpan newts = (TimeSpan) args[0];
                ts = newts;
            });
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register" , "Game.Set.Current.Queue", (object[] args) => 
        {
            return new ActionCommand(() => {
                queue = (Queue<Spaceship__Server.ICommand>)args[0];
            });
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register" , "Game.Current.Queue", (object[] args) => 
        {
            return queue;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register" , "Game.Current.HandleCommand", (object[] args) => 
        {
            Hwdtech.IoC.Resolve<Queue<Spaceship__Server.ICommand>>("Game.Current.Queue").TryDequeue(out cmd!);
            return cmd;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "HandleException", (object [] args) => 
        {
            var err = args[0];
            var command = args[1];
            Dictionary<string, Spaceship__Server.ICommand> subtree = new();

            Mock<Spaceship__Server.ICommand> defaultStrategy = new();

            defaultStrategy.Setup(s => s.Execute()).Callback(() => {throw (System.Exception) err;});
            
            var errtype = err.GetType();

            Mock<Spaceship__Server.ICommand> mcmd = new();

            //var cmd = mcmd.Object;

            Spaceship__Server.ICommand cmd = new ActionCommand(() => {});

            Dictionary<string, Dictionary<string, Spaceship__Server.ICommand>> tree = 
            Hwdtech.IoC.Resolve<Dictionary<string, Dictionary<string, Spaceship__Server.ICommand>>>("Handler.Tree");

            if(tree.TryGetValue(command.ToString()!, out subtree!))
            {
                if(subtree.TryGetValue(errtype.ToString(), out cmd!))
                {
                    return cmd;
                }
            }

            return defaultStrategy.Object;

        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Handler.Tree", (object [] args) =>
        {
            Dictionary<string, Dictionary<string, Spaceship__Server.ICommand>> tree = new();
            
            Spaceship__Server.ICommand HandleStrategy = new ActionCommand(() => { exceptionWasHandled = true;});

            Spaceship__Server.ICommand OtherHandleStrategy = new ActionCommand(() => {});

            Dictionary<string, Spaceship__Server.ICommand> Exceptions = new(){{"System.Exception", HandleStrategy}, {"System.ArgumentException", OtherHandleStrategy}};

            tree = new(){{"Spaceship__Server.ExceptionThrower", Exceptions}, {"Spaceship__Server.MoveCommand", Exceptions}};

            return tree;

        }).Execute();

        return scope;
    }

    [Fact]
    public void ExceptionHandleTest()
    {

        var scope = Init();

        Assert.False(Hwdtech.IoC.Resolve<bool>("Get.Exception.Status"));

        GameCommand Game = new(scope);

        Game.Execute();

        Assert.True(Hwdtech.IoC.Resolve<bool>("Get.Exception.Status"));
    }

    [Fact]
    public void TimeSpanTest()
    {
        var scope = Init();

        GameCommand Game = new(scope);

        Stopwatch timer = new();

        timer.Start();

        Game.Execute();

        Assert.InRange<TimeSpan>(timer.Elapsed, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 0, 0, 200));

        Assert.Empty(Hwdtech.IoC.Resolve<Queue<Spaceship__Server.ICommand>>("Game.Current.Queue"));

        Assert.True(Hwdtech.IoC.Resolve<bool>("Get.Command.Status"));
    }

    [Fact]
    public void ScopeTest()
    {
        var scope = Init();

        GameCommand Game = new(scope);

        Assert.Equal(scope, Game.scope);
    }

    [Fact]
    public void ExceptionThrowerTest()
    {
        var scope = Init();

        Spaceship__Server.ICommand cmd = new ActionCommand(() => 
        {
            throw new Exception();
        });

        Queue<Spaceship__Server.ICommand> queue = new(new[] {cmd});

        Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Set.Current.Queue", queue).Execute();

        Assert.Throws<Exception>(() => 
        {
            GameCommand Game = new(scope);
            Game.Execute();        
        });
    }


}
