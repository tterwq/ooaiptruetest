namespace Spaceship__Server;

public interface ISender
{
    Spaceship__Server.ICommand Send(object message);
}
