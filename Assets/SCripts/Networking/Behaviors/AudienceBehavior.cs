using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudienceBehavior : BaseBehavior
{
    private AudienceInterface interf;
    private ServerNetworkManager serverNetworkManager;

    [HideInInspector]
    [SyncVar]
    public string Name;

    private void Awake()
    {
        interf = FindObjectOfType<AudienceInterface>();
        if (interf)
            interf.NetworkBehavior = this;
        serverNetworkManager = FindObjectOfType<ServerNetworkManager>();
    }

    [Command]
    public void LockAnswer(int index, bool isLifeline)
    {
        if (isLifeline)
        {
            serverNetworkManager.AddAudiencePollAnswerIndex(index);
        }
        else
        {
            serverNetworkManager.AddAudienceAnswer(index, this);
        }
    }

    [TargetRpc]
    public void OnConnected()
    {
        if (!interf)
            return;
        interf.Initialize();
    }

    [TargetRpc]
    public void AskLifelineQuestion(Question question)
    {
        interf.ShowPollOptions(question);
    }

    [TargetRpc]
    public void AskQuestion(Question question)
    {
        interf.ShowQuestion(question);
    }

    [TargetRpc]
    public void ShowCongratulationsMessage()
    {
        interf.ShowCongratulationsMessage();
    }
}
