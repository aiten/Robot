namespace MqttService.Abstraction;

internal class MqttConfig
{
    public string? Broker   { get; set; }
    public int     Port     { get; set; }
    public string? User     { get; set; }
    public string? Password { get; set; }
}