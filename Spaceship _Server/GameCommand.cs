using Hwdtech;
using System;
using System.Diagnostics;
namespace Spaceship__Server;


public class GameCommand : Spaceship__Server.ICommand
{
    public object scope;
    public GameCommand(object scope)
    {
        this.scope = scope;
    }
    public void Execute()
    {
        var parentscope = IoC.Resolve<object>("Scopes.Root");

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", this.scope).Execute();

        Stopwatch timer = new();

        timer.Start();

        while(timer.Elapsed < IoC.Resolve<TimeSpan>("Game.Current.Timespan"))
        {
            try
            {
            
                Spaceship__Server.ICommand cmd = IoC.Resolve<Spaceship__Server.ICommand>("Game.Current.HandleCommand");
                
                if (cmd != null)
                {
                    cmd.Execute();
                }
                else{
                    break;
                }
            }
            catch(Exception e)
            {
                IoC.Resolve<Spaceship__Server.ICommand>("HandleException", e, IoC.Resolve<string>("Get.Exception.Source", e)).Execute();
            }
        }
    }
}
