using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ModeratorBehavior : BaseBehavior
{
    private ModeratorInterface interf;
    private ServerNetworkManager serverNetworkManager;

    [SyncVar]
    public Match CurrentMatch;
    public readonly SyncList<Team> Teams = new SyncList<Team>();


    private void Awake()
    {
        serverNetworkManager = FindObjectOfType<ServerNetworkManager>();
        interf = FindObjectOfType<ModeratorInterface>();
        if (interf)
        {
            interf.NetworkBehavior = this;
        }
    }

    [Command]
    public void GetCurrentMatchDatas(int currentMatchIndex)
    {
        serverNetworkManager.GetCurrentMatchDatas(currentMatchIndex);
    }

    [Command]
    public void RequestLogo(int teamId)
    {
        serverNetworkManager.GetLogo(teamId);
    }

    [Command]
    public void TriggerTeamConnection(List<Team> teams)
    {
        serverNetworkManager.TriggerTeamConnection(teams);
    }

    [Command]
    public void ProceedToMatch()
    {
        serverNetworkManager.ProceedToMatch();
    }

    [Command]
    public void FireQuestion(int teamIndex, Team team, Question question, bool usedLifeline)
    {
        serverNetworkManager.FireQuestion(teamIndex, team, question, usedLifeline);
    }

    [Command]
    public void RefireQuestion(int teamIndex, Team team, Question question)
    {
        serverNetworkManager.RefireQuestion(teamIndex, team, question);
    }

    [Command]
    public void ResetTimer(int teamIndex)
    {
        serverNetworkManager.ResetTimer(teamIndex);
    }

    [Command]
    public void FireShowOptions(int teamIndex)
    {
        serverNetworkManager.FireShowOptions(teamIndex);
    }

    [Command]
    public void FireRevealContestantAnswer(int teamIndex, int lockedAnswerIndex)
    {
        serverNetworkManager.FireRevealContestantAnswer(teamIndex, lockedAnswerIndex);
    }

    [Command]
    public void FireShowLeaderboard()
    {
        serverNetworkManager.ShowLeaderboard();
    }

    [Command]
    public void FireHideLeaderboard()
    {
        serverNetworkManager.HideLeaderboard();
    }

    [Command]
    public void AskQuestionToAudience(Question question)
    {
        serverNetworkManager.AskQuestionToAudience(question);
    }

    [Command]
    public void RevealAudienceAnswer()
    {
        serverNetworkManager.RevealAudienceAnswer();
    }

    [Command]
    public void AcceptExpertLifeline(int teamIndex)
    {
        serverNetworkManager.AcceptExpertLifeline(teamIndex);
    }

    [Command]
    public void AcceptAudienceLifeline(int teamIndex, Question question)
    {
        serverNetworkManager.AcceptAudienceLifeline(teamIndex, question);
    }

    [Command]
    public void AcceptHalfLifeline(int teamIndex, List<int> indexesToRemove)
    {
        serverNetworkManager.AcceptHalfLifeline(teamIndex, indexesToRemove);
    }

    [Command]
    public void SetAudienceCode(string code, bool connectionStart)
    {
        serverNetworkManager.SetAudienceCode(code, connectionStart);
    }

    [Command]
    public void ConfirmRevealPollResult(int teamIndex)
    {
        serverNetworkManager.RevealPollResult(teamIndex);
    }

    [Command]
    public void SetPoints(int teamIndex, int points)
    {
        serverNetworkManager.SetPoints(teamIndex, points);
    }

    [Command]
    public void SetWinner(int winnerIndex)
    {
        serverNetworkManager.SetWinner(winnerIndex);
    }

    [TargetRpc]
    public void OnConnected(Match[] matches)
    {
        if (!interf)
            return;
        interf.Initialize(matches);
    }

    [TargetRpc]
    public void SetLogoData(int teamId, byte[] logoData)
    {
        interf.SetLogoData(teamId, logoData);
    }

    [TargetRpc]
    public void ContestantConnected(int teamIndex)
    {
        interf.OnContestantConnected(teamIndex);
    }

    [TargetRpc]
    public void LockAnswer(int index)
    {
        interf.LockAnswer(index);
    }

    [TargetRpc]
    public void TimeElapsed()
    {
        interf.TimeElapsed();
    }

    [TargetRpc]
    public void RequestExpertLifeline()
    {
        interf.ShowExpertLifelineRequest();
    }

    [TargetRpc]
    public void RequestAudienceLifeline()
    {
        interf.ShowAudienceLifelineRequest();
    }

    [TargetRpc]
    public void RequestSwapLifeline()
    {
        interf.ShowSwapLifelineRequest();
    }

    [TargetRpc]
    public void RequestHalfLifeline()
    {
        interf.ShowHalfLifelineRequest();
    }

    [TargetRpc]
    public void AddAudienceMember(string name)
    {
        interf.AddAudienceMember(name);
    }

    [TargetRpc]
    public void ShowRevealPollResultRequest()
    {
        interf.ShowRevealPollResult();
    }

    [TargetRpc]
    internal void NotifyDisconnection(int teamIndex)
    {
        interf.ShowDisconnectionNotification(teamIndex);
    }
}
