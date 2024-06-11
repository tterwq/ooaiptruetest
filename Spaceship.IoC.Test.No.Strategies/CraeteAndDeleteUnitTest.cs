using Spaceship__Server;

using Hwdtech;
namespace Spaceship.IoC.Test.No.Strategies;
using Hwdtech.Ioc;
using Moq;

public class CreateAndDeleteGameUnitTest
{
	Dictionary<string, object> scopes = new Dictionary<string, object>();
    public CreateAndDeleteGameUnitTest()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
		Hwdtech.IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set",  Hwdtech.IoC.Resolve<object>("Scopes.New", Hwdtech.IoC.Resolve<object>("Scopes.Root"))).Execute();

		var GetScope = new Mock<IStrategy>();
		GetScope.Setup(o => o.run_strategy(It.IsAny<object[]>())).Returns((object[] args) => scopes[(string)args[0]]);
		var NewScopeToDict = new Mock<IStrategy>();
		NewScopeToDict.Setup(o => o.run_strategy(It.IsAny<object[]>())).Returns((object[] args) => {
				scopes.Add((string)args[0], Hwdtech.IoC.Resolve<object>("Scopes.New", (object)args[1]));
				return scopes[(string)args[0]];
				});
		var DeleteScopeFromDict = new Mock<IStrategy>();
		DeleteScopeFromDict.Setup(o => o.run_strategy(It.IsAny<object[]>())).Returns((object[] args) => scopes.Remove((string)args[0]));
		var CreateGameCommand = new Mock<IStrategy>();
		CreateGameCommand.Setup(o => o.run_strategy(It.IsAny<object[]>())).Returns((object[] args) => new Mock<Spaceship__Server.ICommand>().Object);

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Scope.Create.GameCommand", (object[] args) => CreateGameCommand.Object.run_strategy(args)).Execute();
		Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Session.GetScope", (object[] args) => GetScope.Object.run_strategy(args)).Execute();
		Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Session.NewScope", (object[] args) => NewScopeToDict.Object.run_strategy(args)).Execute();
		Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Delete.New", (object[] args) => DeleteScopeFromDict.Object.run_strategy(args)).Execute();
		Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Create.New", (object[] args) => new CreateScopeAndGameStrategy().run_strategy(args)).Execute();

    }

	[Fact]
	public void GoodCreateTwoGamesAndDeleteOneTest()
	{
		var gameCommand = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Create.New", "Game1", "Scope1");
		var gameCommand2 = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Create.New", "Game1", "Scope2");

		Assert.True(scopes.Count == 2);
		Assert.True(scopes["Scope1"] != scopes["Scope2"]);

		Hwdtech.IoC.Resolve<bool>("Game.Delete.New", "Scope1");

		Assert.True(scopes.Count == 1);

		Hwdtech.IoC.Resolve<bool>("Game.Delete.New", "Scope2");

		Assert.True(scopes.Count == 0);
	}

	[Fact]
	public void TestRegisterTimeSpanInGameScope()
	{
		var gameCommand = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Create.New", "Game1", "Scope1");

		var ts = Hwdtech.IoC.Resolve<TimeSpan>("Game.Get.TimeSpan");
		Assert.True(ts == new TimeSpan(100));
	}

	[Fact]
	public void TestRegisterDictOfObjectsInGameScope()
	{
		var gameCommand = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Create.New", "Game1", "Scope1");

		var obj = Hwdtech.IoC.Resolve<IUObject>("Game.Get.Object", "Object1");
		var objDict = Hwdtech.IoC.Resolve<Dictionary<string, IUObject>>("Game.Get.Dict");
		var testobj = new Mock<IUObject>().Object;
		Assert.True(objDict.Count == 1);
		Assert.True(obj.GetType() == testobj.GetType());

		var delFlag = Hwdtech.IoC.Resolve<bool>("Game.Delete.Object", "Object1");
		Assert.True(delFlag == true);
		Assert.True(objDict.Count == 0);
		Assert.Throws<KeyNotFoundException>(() => Hwdtech.IoC.Resolve<IUObject>("Game.Get.Object", "Object1"));
	}

	[Fact]
	public void TestRegisterQueueInGameScope()
	{
		var gameCommand = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Game.Create.New", "Game1", "Scope1");

		var fireCommand = new Mock<Spaceship__Server.ICommand>().Object;

		Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Queue.Enqueue.Command", fireCommand).Execute();
		var cmdQueue = Hwdtech.IoC.Resolve<Queue<Spaceship__Server.ICommand>>("Game.Get.Queue");
		Assert.True(cmdQueue.Count == 1);

		Spaceship__Server.ICommand fireCommandE = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Queue.Dequeue.Command");
		Assert.True(fireCommand == fireCommandE);
		Assert.True(cmdQueue.Count == 0);
	}
}
