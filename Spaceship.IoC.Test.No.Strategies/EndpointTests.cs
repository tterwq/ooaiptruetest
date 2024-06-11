namespace Spaceship.IoC.Test.No.Strategies;
using System.Collections.Concurrent;
using Moq;
using Hwdtech;
using Spaceship__Server;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Newtonsoft;


public class EndTests
{
   [Fact]
    public object CreateIoCDependencies()
    {
        new Hwdtech.Ioc.InitScopeBasedIoCImplementationCommand().Execute();

        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        Dictionary<string, MyThread> GameThreads = new();

        Dictionary<string, ISender> GameSenders = new();

        Dictionary<string, Dictionary<string, IUObject>> GamesObjects = new();

        Dictionary<string, IUObject> game1 = new();

        Mock<IUObject> obj = new();

        Mock<IUObject> _obj = new();

        Queue<Spaceship__Server.ICommand> _queue = new();

        _obj.Setup(o => o.get_property("Velocity")).Returns((object) new Vector(1, 1));

        _obj.Setup(o => o.get_property("Position")).Returns((object) new Vector(1, 1));

        game1.Add("obj123", _obj.Object);

        GamesObjects.Add("1", game1);

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

                GameThreads.TryAdd(id, thread);
                GameSenders.TryAdd(id, sender);

                return thread;
            }
            else{
                string id = (string)args[0]; 
                BlockingCollection<Spaceship__Server.ICommand> q = new();

                ISender sender = new SenderAdapter(q);
                IReciver receiver = new RecieverAdapter(q);
                MyThread thread = new(receiver);

                thread.Start();

                GameThreads.TryAdd(id, thread);
                GameSenders.TryAdd(id, sender);

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

         Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Adapters.IUObject.Movable", (object[] args) => 
        {
            var obj = (IUObject)args[0];
            MovableAdapter adp = new MovableAdapter(obj);
            return adp;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ContiniousMovement.Get.Dependencies", (object[] args) =>
        {
            List<string> deps = new List<string>{"MoveCommand"};
            return deps;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "IoC.MoveCommand", (object[] args) =>
        {
            return (Spaceship__Server.ICommand) new MoveCommand(Hwdtech.IoC.Resolve<IMovable>("Adapters.IUObject.Movable", args));
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "IoC.CreateMacro", (object[] args) =>
        {
            MacroCreator creator = new();
            return creator.CreateMacro(args);
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get Object by ids", (object[] args) => 
        {
            string GameID = (string) args[0];

            string ObjectID = (string) args[1];

            IUObject obj = GamesObjects[GameID][ObjectID];

            return obj;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Stop Move Command", (object[] args) => 
        {
            IUObject obj = (IUObject) args[0];

            Mock<Spaceship__Server.ICommand> cmd = new();

            return cmd.Object;

        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Start Rotation Command", (object[] args) => 
        {
            IUObject obj = (IUObject) args[0];

            Mock<Spaceship__Server.ICommand> cmd = new();

            return cmd.Object;

        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Shoot Command", (object[] args) => 
        {
            IUObject obj = (IUObject) args[0];

            Mock<Spaceship__Server.ICommand> cmd = new();

            return cmd.Object;

        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Deserialize Message to Command", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            string MessageType = (string) MessageContent["type"];

            Spaceship__Server.ICommand cmd = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Create " + MessageType + " by Message", MessageContent);

            string ThreadID = (string) MessageContent["thread"];

            return ((Spaceship__Server.ICommand)Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Send Command", ThreadID, cmd));
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create StartMove by Message", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            IUObject obj = Hwdtech.IoC.Resolve<IUObject>("Get Object by ids", MessageContent["gameid"], MessageContent["objid"]);

            Spaceship__Server.ICommand cmd = IoC.Resolve<Spaceship__Server.ICommand>("IoC.CreateMacro", "ContiniousMovement", obj);

            return cmd;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create AssertCommand by Message", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            Spaceship__Server.ICommand cmd = new ActionCommand(() => 
            {
                MyThread thread = Hwdtech.IoC.Resolve<MyThread>("Get thread by id", "2");
                IReciver reciver = thread.receiver;
                Assert.False(reciver.isEmpty());
            });

            return cmd;
        }).Execute();

        Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "1");

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create StopMove by Message", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            IUObject obj = Hwdtech.IoC.Resolve<IUObject>("Get Object by ids", MessageContent["gameid"], MessageContent["objid"]);

            Spaceship__Server.ICommand cmd = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Stop Move Command", obj);

            return cmd;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create StartRotate by Message", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            IUObject obj = Hwdtech.IoC.Resolve<IUObject>("Get Object by ids", MessageContent["gameid"], MessageContent["objid"]);

            Spaceship__Server.ICommand cmd = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Start Rotation Command", obj);

            return cmd;
        }).Execute();

        Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Create Shoot by Message", (object[] args) => 
        {
            Dictionary<string, object> MessageContent = (Dictionary<string, object>) args[0];

            IUObject obj = Hwdtech.IoC.Resolve<IUObject>("Get Object by ids", MessageContent["gameid"], MessageContent["objid"]);

            Spaceship__Server.ICommand cmd = Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Shoot Command", obj);

            return cmd;
        }).Execute();

        return scope;
    }
    [Fact (Skip = "Bad test")]
    public void CreateMoveCommand()
    {
        var scope = Dependencies.Run();

        Dictionary<string, object> ValueDictionary = new(){{"type", "StartMove"}, {"gameid", "1"}, {"objid", "obj123"}, {"thread", "2"}, {"velocity", 1}};

        Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2");

        JsonDictionary Value = new(ValueDictionary);

        JSONContract Contract = new();

        Contract.Value = Value;

        SpaceshipApi Endpoint = new();

        IUObject obj = Hwdtech.IoC.Resolve<IUObject>("Get Object by ids", "1", "obj123");

        Assert.Equal(new Vector(1, 1), (Vector)((IUObject)obj.get_property("Object")).get_property("Velocity"));

        AutoResetEvent waiter = new(false);

        Spaceship__Server.ICommand waitCommand = new ActionCommand(() => 
        {
            waiter.Set();
        });

        Endpoint.Message(Contract);

        Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "2", waitCommand).Execute();

        waiter.WaitOne();

        Assert.Equal(new Vector(1, 1), (Vector)((IUObject)obj.get_property("Object")).get_property("Position"));
    }

    [Fact]
    public void CreateStopCommand()
    {
        var scope = Dependencies.Run();

        Dictionary<string, object> ValueDictionary = new(){{"type", "StopMove"}, {"gameid", "1"}, {"objid", "obj123"}, {"thread", "2"}};

        ManualResetEvent waiter123 = new(false);

        Spaceship__Server.ICommand waitCommand = new ActionCommand(() => 
        {
            waiter123.WaitOne();
        });

        Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2", () => {waitCommand.Execute();});

        JsonDictionary Value = new(ValueDictionary);

        JSONContract Contract = new();

        Contract.Value = Value;

        SpaceshipApi Endpoint = new();

        MyThread thread = Hwdtech.IoC.Resolve<MyThread>("Get thread by id", "2");

        IReciver reciver = thread.receiver;

        Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "2", waitCommand).Execute();

        Endpoint.Message(Contract);

        Assert.False(reciver.isEmpty());

        waiter123.Set();

    }

    [Fact]
    public void CreateRotateCommand()
    {
        var scope = Dependencies.Run();

        Dictionary<string, object> ValueDictionary = new(){{"type", "StartRotate"}, {"gameid", "1"}, {"objid", "obj123"}, {"thread", "2"}};

        ManualResetEvent waiter123 = new(false);

        Spaceship__Server.ICommand waitCommand = new ActionCommand(() => 
        {
            waiter123.WaitOne();
        });

        Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2", () => {waitCommand.Execute();});

        JsonDictionary Value = new(ValueDictionary);

        JSONContract Contract = new();

        Contract.Value = Value;

        SpaceshipApi Endpoint = new();

        MyThread thread = Hwdtech.IoC.Resolve<MyThread>("Get thread by id", "2");

        IReciver reciver = thread.receiver;

        Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "2", waitCommand).Execute();

        Endpoint.Message(Contract);

        Assert.False(reciver.isEmpty());

        waiter123.Set();
    }

    [Fact]
    public void CreateShootCommand()
    {
        var scope = Dependencies.Run();

        Dictionary<string, object> ValueDictionary = new(){{"type", "Shoot"}, {"gameid", "1"}, {"objid", "obj123"}, {"thread", "2"}};

        ManualResetEvent waiter123 = new(false);

        Spaceship__Server.ICommand waitCommand = new ActionCommand(() => 
        {
            waiter123.WaitOne();
        });

        Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2", () => {waitCommand.Execute();});

        JsonDictionary Value = new(ValueDictionary);

        JSONContract Contract = new();

        Contract.Value = Value;

        SpaceshipApi Endpoint = new();

        MyThread thread = Hwdtech.IoC.Resolve<MyThread>("Get thread by id", "2");

        IReciver reciver = thread.receiver;

        Hwdtech.IoC.Resolve<Spaceship__Server.ICommand>("Send Command", "2", waitCommand).Execute();

        Endpoint.Message(Contract);

        Assert.False(reciver.isEmpty());

        waiter123.Set();
    }
    [Fact]
    public void InitTest()
    {
        SpaceshipApi Endpoint = new();

        Endpoint.Init();

        Assert.IsType<MyThread>(Hwdtech.IoC.Resolve<MyThread>("Get thread by id", "2"));
    }

    [Fact]
    public void ContractTest()
    {
        Dictionary<string, object> ValueDictionary = new(){{"type", "Shoot"}, {"gameid", "1"}, {"objid", "obj123"}, {"thread", "2"}};

        JSONContract dto =  new();

        JsonDictionary jd = new(ValueDictionary);

        JsonDictionary jd2 = new();

        FormatterConverter fc = new();

        SerializationInfo si = new(typeof(JsonDictionary), fc);

        StreamingContext sc = new();

        si.AddValue("type123", "Shoot");

        JsonDictionary jd3 = new(ValueDictionary);
        
        dto.Value = jd;

        MemoryStream mem = new MemoryStream();
        try
        {   

            jd3.GetObjectData(si, sc);
            JsonDictionary jd4 = new(si, sc);

            var serobj = JsonSerializer.Serialize(dto);
            var jsonobj = JsonSerializer.Deserialize<JSONContract>(serobj);

            var serobj2 = JsonSerializer.Serialize(jd);
            var jsonobj2 = JsonSerializer.Deserialize<JsonDictionary>(serobj2);

            var serobj3 = JsonSerializer.Serialize(jd2);
            var jsonobj3 = JsonSerializer.Deserialize<JsonDictionary>(serobj3);

            Assert.IsType<JSONContract>(jsonobj);
            Assert.IsType<JsonDictionary>(jsonobj2);
            Assert.IsType<JsonDictionary>(jsonobj3);

        }
        catch (Exception ex)
        {
            Assert.Empty(ex.Message);
        }
    }
}
// Идея создать эндпоинт который будет регистроровать зависимости как для тестов и вызывать его до вызова команд с мессаджами