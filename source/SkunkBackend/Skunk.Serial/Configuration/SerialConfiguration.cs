namespace Skunk.Serial.Configuration;

public class SerialConfiguration
{
    public string ComPortName { get; init; } = "/dev/ttyACM0";
    public int BaudRate { get; init; } = 9600;
}
