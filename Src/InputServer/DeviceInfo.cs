namespace InputServer;

using System;

public record DeviceInfo(string Name, Guid? Guid, string Type, string SendTo);