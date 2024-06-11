using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spaceship__Server
{
    public interface IMovable
    {
        Vector Speed { get; }
        Vector Position { get; set; }
    }

    public class MoveCommand : ICommand
    {
        IMovable _obj;
        public MoveCommand(IMovable obj)
        {
            _obj = obj;
        }

        public void Execute()
        {
            if (_obj.Speed.coords.Count != _obj.Position.coords.Count)
            {
                throw new Exception();
            }
            Vector newpos = _obj.Position;
            for (int i = 0; i < _obj.Position.coords.Count; i++)
            {
                newpos[i] += _obj.Speed[i];
            }
            _obj.Position = newpos;
        }
    }
}
