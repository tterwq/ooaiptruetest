namespace Spaceship__Server;


public interface IReciver
{
    ICommand Receive();
    bool isEmpty();
}
