namespace Spaceship__Server;

public interface IRotatable
{
    Angle angle
    {
        get;
        set;
    }

    Angle angle_speed
    {
        get;
    }
}
