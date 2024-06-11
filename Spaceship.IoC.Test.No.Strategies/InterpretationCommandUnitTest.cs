namespace Spaceship.IoC.Test.No.Strategies;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Spaceship__Server;


public class TestInterpretCommand
{

    public TestInterpretCommand()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Create.CommandFromMessage", (object[] args) => new MessageToCommand().run_strategy(args)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Message.Get.IUObject", (object[] args) => new GetIUObject().run_strategy(args)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.Queue", (object[] args) => new GetQueue().run_strategy(args)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Queue.Push", (object[] args) => new PushToQueue().run_strategy(args)).Execute();
    }

    [Fact]
    public void GoodPushToQueue()
    {
        Dictionary<string, Queue<Spaceship__Server.ICommand>> DictGames = new Dictionary<string, Queue<Spaceship__Server.ICommand>>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.GameDictionary", (object[] args) => DictGames).Execute();

        Dictionary<string, IUObject> DictObj = new Dictionary<string, IUObject>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.UObjectsDictionary", (object[] args) => DictObj).Execute();

        Mock<Spaceship__Server.ICommand> mockCommand = new Mock<Spaceship__Server.ICommand>();
        Mock<IUObject> mockUObject = new Mock<IUObject>();

        mockUObject.Setup(x => x.set_property(It.IsAny<string>(), It.IsAny<object>()));

        DictGames.Add("asdfg", new Queue<Spaceship__Server.ICommand>());

        DictObj.Add("548", mockUObject.Object);

        Mock<IMessage> mockMessage = new Mock<IMessage>();
        mockMessage.SetupGet(x => x.gameId).Returns("asdfg");
        mockMessage.SetupGet(x => x.cmd).Returns("Fire");
        mockMessage.SetupGet(x => x.properties).Returns(new Dictionary<string, object> { { "Damage", 10 } });
        mockMessage.SetupGet(x => x.objId).Returns("548");

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.FireCommand", (object[] args) => mockCommand.Object).Execute();

        var interpreted = new InterpretationCommand(mockMessage.Object);
        interpreted.Execute();

        Assert.True(DictGames["asdfg"].Count == 1);
    }

    [Fact]
    public void GetExceptionFromGameId()
    {
        Dictionary<string, Queue<Spaceship__Server.ICommand>> DictGames = new Dictionary<string, Queue<Spaceship__Server.ICommand>>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.GameDictionary", (object[] args) => DictGames).Execute();

        Dictionary<string, IUObject> DictObj = new Dictionary<string, IUObject>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.UObjectsDictionary", (object[] args) => DictObj).Execute();

        Mock<Spaceship__Server.ICommand> mockCommand = new Mock<Spaceship__Server.ICommand>();
        Mock<IUObject> mockUObject = new Mock<IUObject>();

        mockUObject.Setup(x => x.set_property(It.IsAny<string>(), It.IsAny<object>()));

        DictGames.Add("asdfg", new Queue<Spaceship__Server.ICommand>());

        DictObj.Add("548", mockUObject.Object);

        Mock<IMessage> mockMessage = new Mock<IMessage>();
        mockMessage.SetupGet(x => x.gameId).Returns("qwerty"); // Get bad Game id
        mockMessage.SetupGet(x => x.cmd).Returns("Fire");
        mockMessage.SetupGet(x => x.properties).Returns(new Dictionary<string, object> { { "Damage", 10 } });
        mockMessage.SetupGet(x => x.objId).Returns("548");

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.FireCommand", (object[] args) => mockCommand.Object).Execute();

        var interpreted = new InterpretationCommand(mockMessage.Object);
        Assert.Throws<Exception>(() => { interpreted.Execute(); });
    }

    [Fact]
    public void GetExceptionFromIUObjectId()
    {
        Dictionary<string, Queue<Spaceship__Server.ICommand>> DictGames = new Dictionary<string, Queue<Spaceship__Server.ICommand>>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.GameDictionary", (object[] args) => DictGames).Execute();

        Dictionary<string, IUObject> DictObj = new Dictionary<string, IUObject>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.UObjectsDictionary", (object[] args) => DictObj).Execute();

        Mock<Spaceship__Server.ICommand> mockCommand = new Mock<Spaceship__Server.ICommand>();
        Mock<IUObject> mockUObject = new Mock<IUObject>();

        mockUObject.Setup(x => x.set_property(It.IsAny<string>(), It.IsAny<object>()));

        DictGames.Add("asdfg", new Queue<Spaceship__Server.ICommand>());

        DictObj.Add("548", mockUObject.Object);

        Mock<IMessage> mockMessage = new Mock<IMessage>();
        mockMessage.SetupGet(x => x.gameId).Returns("asdfg");
        mockMessage.SetupGet(x => x.cmd).Returns("Fire");
        mockMessage.SetupGet(x => x.properties).Returns(new Dictionary<string, object> { { "Damage", 10 } });
        mockMessage.SetupGet(x => x.objId).Returns("777");  //Get bad Object id

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.FireCommand", (object[] args) => mockCommand.Object).Execute();

        var interpreted = new InterpretationCommand(mockMessage.Object);
        Assert.Throws<Exception>(() => { interpreted.Execute(); });
    }

    [Fact]
    public void GetExceptionFromBadProperties()
    {
        Dictionary<string, Queue<Spaceship__Server.ICommand>> DictGames = new Dictionary<string, Queue<Spaceship__Server.ICommand>>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.GameDictionary", (object[] args) => DictGames).Execute();

        Dictionary<string, IUObject> DictObj = new Dictionary<string, IUObject>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.UObjectsDictionary", (object[] args) => DictObj).Execute();

        Mock<Spaceship__Server.ICommand> mockCommand = new Mock<Spaceship__Server.ICommand>();
        Mock<IUObject> mockUObject = new Mock<IUObject>();

        mockUObject.Setup(x => x.set_property(It.IsAny<string>(), It.IsAny<object>()));

        DictGames.Add("asdfg", new Queue<Spaceship__Server.ICommand>());

        DictObj.Add("548", mockUObject.Object);

        Mock<IMessage> mockMessage = new Mock<IMessage>();
        mockMessage.SetupGet(x => x.gameId).Returns("asdfg");
        mockMessage.SetupGet(x => x.cmd).Returns("Fire");
        mockMessage.SetupGet(x => x.properties).Throws(new Exception()); // Bad property
        mockMessage.SetupGet(x => x.objId).Returns("777");

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Get.FireCommand", (object[] args) => mockCommand.Object).Execute();

        var interpreted = new InterpretationCommand(mockMessage.Object);
        Assert.Throws<Exception>(() => { interpreted.Execute(); });
    }
}

