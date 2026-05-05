using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudienceNetworkManager : BaseNetworkManager
{
    [HideInInspector]
    public string Code;
    [HideInInspector]
    public string Name;

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        ConnectAsAudience(Code, Name);
    }

    public override void OnClientDisconnect()
    {
        NetworkClient.Shutdown();
    }

    public void ConnectAsAudience(string teamCode, string name)
    {
        AudienceConnectionMessage message = new AudienceConnectionMessage();
        message.Code = teamCode;
        message.Name = name;
        NetworkClient.connection.Send(message);
    }
}
