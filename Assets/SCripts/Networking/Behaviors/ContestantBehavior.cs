using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ContestantBehavior : BaseBehavior
{
    private ContestantInterface interf;
    private ServerNetworkManager serverNetworkManager;

    private void Awake()
    {
        interf = FindObjectOfType<ContestantInterface>();
        serverNetworkManager = FindObjectOfType<ServerNetworkManager>();
    }

    [Command]
    public void LockAnswer(int index)
    {
        serverNetworkManager.SetAnswerIndex(index);
    }

    [Command]
    public void TimeElapsed()
    {
        serverNetworkManager.TimeElapsed();
    }

    [Command]
    public void RequestExpertLifeline()
    {
        serverNetworkManager.RequestExpertLifeline();
    }
    [Command]
    public void RequestAudienceLifeline()
    {
        serverNetworkManager.RequestAudienceLifeline();
    }
    [Command]
    public void RequestSwapLifeline()
    {
        serverNetworkManager.RequestSwapLifeline();
    }
    [Command]
    public void RequestHalfLifeline()
    {
        serverNetworkManager.RequestHalfLifeline();
    }

    [TargetRpc]
    public void OnConnected()
    {
        if (!interf)
            return;
        interf.Initialize();
    }

    [TargetRpc]
    public void SetLogoData(int teamId, byte[] logoData)
    {
        interf.SetLogoData(teamId, logoData);
    }

    [TargetRpc]
    public override void SendError(string error)
    {
        base.SendError(error);
        print("Test");
        StartCoroutine(interf.HideDisconnectedScreen(.1f));
    }

    [TargetRpc]
    public void ShowQuestion(Question question, Team team, List<Team> teams)
    {
        interf.ShowQuestion(question, team, teams);
    }

    [TargetRpc]
    public void ResetTimer()
    {
        interf.ResetTimer();
    }

    [TargetRpc]
    public void SetTimer(float time)
    {
        interf.SetTimer(time);
    }

    [TargetRpc]
    public void ShowOptions()
    {
        interf.ShowOptions();
    }

    [TargetRpc]
    public void RevealAnswer()
    {
        interf.RevealAnswer();
    }

    [TargetRpc]
    public void ApplyExpertLifeline()
    {
        interf.ApplyExpertLifeline();
    }

    [TargetRpc]
    public void ApplyAudienceLifeline()
    {
        interf.ApplyAudienceLifeline();
    }

    [TargetRpc]
    public void ApplyHalfLifeline(List<int> indexesToRemove)
    {
        interf.ApplyHalfLifeline(indexesToRemove);
    }

    [TargetRpc]
    public void RevealPollResult(List<int> audiencePollResults)
    {
        interf.RevealPollResult(audiencePollResults);
    }

    [TargetRpc]
    public void ShowWinnerScreen()
    {
        interf.ShowWinnerScreen();
    }
}
