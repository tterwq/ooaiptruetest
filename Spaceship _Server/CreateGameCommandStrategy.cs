namespace Spaceship__Server;

using Hwdtech;
using Moq;

public class CreateGameCommandStrategy : IStrategy
{
	public object run_strategy(params object[] args)
	{
		object currScope = (object)args[0];

		IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", currScope).Execute(); 

        return IoC.Resolve<ICommand>("Scope.Create.GameCommand");
	}
}
