using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ModeratorNetworkManager : BaseNetworkManager
{
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        ModeratorConnectionMessage message = new ModeratorConnectionMessage();
        NetworkClient.connection.Send(message);
    }

    public override void OnClientDisconnect()
    {
        NetworkClient.Shutdown();
    }
}
