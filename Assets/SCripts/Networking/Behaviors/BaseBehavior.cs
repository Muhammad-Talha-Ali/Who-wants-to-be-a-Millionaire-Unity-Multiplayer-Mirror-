using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BaseBehavior : NetworkBehaviour
{
    [TargetRpc]
    public virtual void SendError(string error)
    {
        print(error);
        if (NetworkClient.active)
        {
            NetworkClient.Disconnect();
        }
    }
}
