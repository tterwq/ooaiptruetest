using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using Hwdtech;
using System;
using System.Threading;
using System.Linq;

namespace Spaceship__Server;


public class Dependencies
{
    public static object Run()
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

        obj.Setup(o => o.get_property("Object")).Returns((object) _obj.Object);

        obj.Setup(o => o.get_property("Queue")).Returns((object) _queue);

        game1.Add("obj123", obj.Object);

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

         Hwdtech.IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Adapters.IUObject.Movable", (object[] args) => 
        {
            MovableAdapter adp = new MovableAdapter(args);
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

        //Hwdtech.IoC.Resolve<MyThread>("Create and Start Thread", "2");

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
}
