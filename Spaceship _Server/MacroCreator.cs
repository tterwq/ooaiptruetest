
using Hwdtech;
using System.Collections.Generic;


namespace Spaceship__Server;

public class MacroCreator
{
    public ICommand CreateMacro(object[] args)
    {
        string call = (string) args[0]+".Get.Dependencies";
        IUObject obj = (IUObject) args[1];
        List<string> dependencies = IoC.Resolve<List<string>>(call);
        List<ICommand> jobs = new();

        foreach(string dependency in dependencies)
        {
            jobs.Add(IoC.Resolve<ICommand>("IoC."+dependency, args[1]));
        }

        return new MacroCommand((Queue<Spaceship__Server.ICommand>)obj.get_property("Queue"), jobs);
        
    }
}
