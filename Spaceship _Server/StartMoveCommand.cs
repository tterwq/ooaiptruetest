using Spaceship__Server;
using System.Collections.Generic;
using Hwdtech;

namespace Spaceship__Server
{
    public interface IMoveCommandStartable : ICommand
    {
        IUObject _obj {set; get;}
        Vector Velocity{get; set;}
        Queue<Spaceship__Server.ICommand> _queue{get;set;}
    };

    public class StartMoveCommand : IMoveCommandStartable
    {
        public IUObject _obj{set; get;}
        public Vector Velocity{get; set;}
        public Queue<Spaceship__Server.ICommand> _queue{set;get;} = new Queue<ICommand>();
        public List<ICommand> jobs {get; set;} = new List<ICommand>();
        public StartMoveCommand(IUObject order)
        {
            this._obj = (IUObject) order.get_property("Object");
            this.Velocity = (Vector) order.get_property("Velocity");
            this._queue = (Queue<Spaceship__Server.ICommand>)order.get_property("Queue");
        }
        public void Execute()
        {
            this._obj.set_property("Velocity", Velocity);

            IMovable movable = Hwdtech.IoC.Resolve<IMovable>("Adapters.IUObject.MoveCommand", _obj);

            ICommand mc = new MoveCommand(movable);

            jobs.Add(mc);

            jobs.Add(new MacroCommand(_queue, new List<ICommand>(){mc}));

            _queue.Enqueue(jobs[0]);
            
            _queue.Enqueue(jobs[1]);
        }
    };
}
