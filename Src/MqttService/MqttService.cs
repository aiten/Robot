namespace MqttService;

using System.Collections.Generic;
using System.Text.RegularExpressions;

using global::MqttService.Abstraction;

public class MqttService
{
    public MqttService()
    {
    }

    public ICollection<string> SubscribeTo { get; set; } = new List<string>();

    public void AddSubscribeToStatAndCmd()
    {
        SubscribeTo.Add(MqttConst.TOPIC_STAT + "/#");
        SubscribeTo.Add(MqttConst.TOPIC_CMND + "/#");
    }

    protected (string name, string tag)? AnalyseTopic(string prefix, string topic)
    {
        // e.g.: robot/cmnd/Robot4WD/drive

        var regEx = new Regex($"{prefix}/(.*)/(.*)");

        var match = regEx.Match(topic);

        if (match.Success)
        {
            string name = match.Groups[1].Value;
            string tag  = match.Groups[2].Value;
            return (name, tag);
        }

        return null;
    }
}