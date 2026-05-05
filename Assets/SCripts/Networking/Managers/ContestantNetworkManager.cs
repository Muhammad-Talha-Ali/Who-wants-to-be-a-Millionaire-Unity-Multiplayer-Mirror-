using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

public class ContestantNetworkManager : BaseNetworkManager
{
    private ContestantInterface interf;

    [HideInInspector]
    public string Code;

    public override void Awake()
    {
        base.Awake();
        interf = FindObjectOfType<ContestantInterface>();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        ConnectToTeam(Code);
    }

    public void ConnectToTeam(string teamCode)
    {
        ContestantConnectionMessage message = new ContestantConnectionMessage();
        message.Code = teamCode;
        NetworkClient.connection.Send(message);
    }

    public override void OnClientDisconnect()
    {
        print("Client disconnected");
        interf.ShowDisconnectedScreen();
        Destroy(FindObjectOfType<ContestantBehavior>().gameObject);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        print("Client stopped");
        interf.HideDisconnectedScreen();
    }

    public void AttemptReconnect()
    {
        NetworkClient.Connect(networkAddress);
        //StartCoroutine(Reconnect());
    }

    private IEnumerator Reconnect()
    {
        NetworkClient.Shutdown();
        yield return new WaitForSeconds(0.3f);
        StartClient();
    }
}
