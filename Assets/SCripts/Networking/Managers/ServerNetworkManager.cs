using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Networking;

public class ServerNetworkManager : BaseNetworkManager
{
    #region PARAMETERS

    [SerializeField] private GameObject moderatorPlayerPrefab;
    [SerializeField] private GameObject contestantPlayerPrefab;
    [SerializeField] private GameObject audiencePlayerPrefab;

    #endregion

    #region CACHES

    private ServerInterface interf;

    #endregion

    #region STATES

    public Match[] matches;

    // Moderator
    [HideInInspector]
    public ModeratorBehavior Moderator;

    // Contestants
    [HideInInspector]
    public Dictionary<int, ContestantBehavior> Contestants = new Dictionary<int, ContestantBehavior>();

    // Audience
    private string audienceCode;
    private List<AudienceBehavior> audienceMembers = new List<AudienceBehavior>();
    private List<AudienceBehavior> audienceWinners;
    private Question audienceQuestion;
    private List<int> audiencePollResults;
    private bool pollInProgress = false;


    Dictionary<int, byte[]> teamsLogos = new Dictionary<int, byte[]>();

    #endregion

    public override void Awake()
    {
        base.Awake();
        interf = FindObjectOfType<ServerInterface>();
        GetMatches();
    }

    private async void GetMatches()
    {
        await ApiManager.Instance.Authenticate();
        matches = await ApiManager.Instance.FetchMatches();
        StartCoroutine(GetLogos(matches));
        print("Ended fetching");
    }

    private IEnumerator GetLogos(Match[] matches)
    {
        foreach (Match match in matches)
        {
            foreach (Team team in match.teams)
            {
                if (teamsLogos.ContainsKey(team.id)) { continue; }
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(team.logo))
                {
                    yield return www.SendWebRequest();
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        Texture2D texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
                        byte[] data = texture2D.EncodeToPNG();
                        teamsLogos.Add(team.id, data);
                    }
                }
            }
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<ModeratorConnectionMessage>(ModeratorConnectionHandler);
        NetworkServer.RegisterHandler<ContestantConnectionMessage>(ContestantConnectionHandler);
        NetworkServer.RegisterHandler<AudienceConnectionMessage>(AudienceConnectionHandler);
    }

    public void ProceedToMatch()
    {
        interf.ProceedToMatch(Moderator.Teams);
    }

    public void SetAudienceCode(string code, bool connectionStart)
    {
        audienceCode = code;
        if (connectionStart)
        {
            interf.StartAudienceConnection(code);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        // If contestant has disconnected
        ContestantBehavior contestant = conn.identity.GetComponent<ContestantBehavior>();
        if (contestant && Contestants.ContainsValue(contestant))
        {
            foreach (KeyValuePair<int, ContestantBehavior> tmp in Contestants)
            {
                if (tmp.Value == contestant)
                {
                    NotifyDisconnection(tmp.Key - 1);
                    Contestants.Remove(tmp.Key);
                    break;
                }
            }
        }

        Destroy(conn.identity.gameObject);
    }

    private void NotifyDisconnection(int teamIndex)
    {
        Moderator.NotifyDisconnection(teamIndex);
    }

    public void GetLogo(int teamId)
    {
        Moderator.SetLogoData(teamId, teamsLogos[teamId]);
    }

    #region CONNECTION HANDLERS

    private void ModeratorConnectionHandler(NetworkConnection conn, ModeratorConnectionMessage message)
    {
        // Instantiate moderator gameobject
        GameObject moderatorPlayer = Instantiate(moderatorPlayerPrefab);
        moderatorPlayer.transform.name = "Moderator";
        // Add moderator to the connection
        NetworkServer.AddPlayerForConnection(conn, moderatorPlayer);
        moderatorPlayer.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        // Initialize moderator
        Moderator = moderatorPlayer.GetComponent<ModeratorBehavior>();
        Moderator.OnConnected(matches);
    }

    private void ContestantConnectionHandler(NetworkConnection conn, ContestantConnectionMessage message)
    {
        // Instantiate contestant gameobject
        GameObject contestantPlayer = Instantiate(contestantPlayerPrefab);
        contestantPlayer.transform.name = "Contestant";
        // Add contestant to the connection
        NetworkServer.AddPlayerForConnection(conn, contestantPlayer);
        contestantPlayer.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        // Initialize contestant
        ContestantBehavior contestant = conn.identity.GetComponent<ContestantBehavior>();
        // Search for the team with corresponding code
        Team contestantTeam = null;
        int teamIndex = 0;
        foreach (Team team in Moderator.Teams)
        {
            if (team.code == int.Parse(message.Code))
            {
                contestantTeam = team;
                break;
            }
            teamIndex++;
        }
        // If the team exists (code is correct)
        if (contestantTeam != null)
        {
            contestant.OnConnected();
            // Add contestant to the contestants dictionary
            if (!Contestants.ContainsKey(contestantTeam.id))
                Contestants.Add(contestantTeam.id, contestant);
            // Notify server and moderator that contestant is connected
            interf.OnContestantConnected(teamIndex);
            Moderator.ContestantConnected(teamIndex);
            // Send teams logos to contestant
            foreach (Team team in Moderator.Teams)
            {
                contestant.SetLogoData(team.id, teamsLogos[team.id]);
            }
            // If a question has been fired before the connection 
            // in case of reconnection after a connection loss
            if (interf.QuestionFired && interf.CurrentTeamId == contestantTeam.id)
            {
                // Show the question on the contestant and main screen
                List<Team> teams = new List<Team>();
                foreach (Team moderatorTeam in Moderator.Teams)
                {
                    teams.Add(moderatorTeam);
                }
                contestant.ShowQuestion(interf.CurrentQuestion, contestantTeam, teams);
                // If options have already been shown
                if (interf.OptionsFired)
                {
                    contestant.ShowOptions();
                    contestant.SetTimer(interf.QuestionForm.GetTimer());
                }
            }
        }
        // If code is incorrect
        else
        {
            contestant.SendError("Incorrect code");
        }
    }

    public async void GetCurrentMatchDatas(int currentMatchIndex)
    {
        Match currentMatch = await ApiManager.Instance.GetCurrentMatchDatas(matches[currentMatchIndex].id);
        Moderator.CurrentMatch = currentMatch;
    }

    private void AudienceConnectionHandler(NetworkConnection conn, AudienceConnectionMessage message)
    {
        // Instantiate moderator gameobject
        GameObject audiencePlayer = Instantiate(audiencePlayerPrefab);
        audiencePlayer.transform.name = "Audience";
        // Add audience to the connection
        NetworkServer.AddPlayerForConnection(conn, audiencePlayer);
        audiencePlayer.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
        // Initialize audience member
        AudienceBehavior audience = conn.identity.GetComponent<AudienceBehavior>();
        audience.Name = message.Name;
        // If code is correct
        if (message.Code.Equals(audienceCode))
        {
            audience.OnConnected();
            audienceMembers.Add(audience);
            Moderator.AddAudienceMember(message.Name);
            interf.AddAudienceMember(message.Name);
        }
        // If code is incorrect
        else
        {
            audience.SendError("Incorrect code");
        }
    }

    #endregion

    #region TEAM CONNECTION SCREEN

    public void TriggerTeamConnection(List<Team> teams)
    {
        Team[] teamsArray = teams.ToArray();
        StartCoroutine(ShowTeamConnection(teamsArray));
    }

    IEnumerator ShowTeamConnection(Team[] teams)
    {
        yield return StartCoroutine(ApiManager.Instance.LoadTeamLogos(teams));

        foreach (Team team in teams)
        {
            Moderator.Teams.Add(team);
        }
        interf.TriggerTeamConnection(Moderator.Teams);
    }

    #endregion

    #region CONTESTANT QUESTIONS

    public void FireQuestion(int teamIndex, Team team, Question question, bool usedLifeline)
    {
        // Create a copy of the team for the synclist to be updated
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        // If the question firing is caused by a swap lifeline
        if (usedLifeline)
        {
            copy.swapLifeline = false;
            team.swapLifeline = false;
        }
        else
        {
            copy.questionsNumber++;
        }
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        // Update synclist
        Moderator.Teams[teamIndex] = copy;
        // Show the question on the contestant and main screen
        List<Team> teams = new List<Team>();
        foreach (Team moderatorTeam in Moderator.Teams)
        {
            teams.Add(moderatorTeam);
        }
        contestant.ShowQuestion(question, team, teams);
        interf.ShowQuestion(team, question, Moderator.Teams);
    }

    public void RefireQuestion(int teamIndex, Team team, Question question)
    {
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        // Show the question on the contestant and main screen
        List<Team> teams = new List<Team>();
        foreach (Team moderatorTeam in Moderator.Teams)
        {
            teams.Add(moderatorTeam);
        }
        contestant.ShowQuestion(question, team, teams);
        interf.ShowQuestion(team, question, Moderator.Teams);
    }

    public void FireShowOptions(int teamIndex)
    {
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.ShowOptions();
        interf.ShowOptions();
    }

    public void SetAnswerIndex(int index)
    {
        interf.LockAnswer(index);
        Moderator.LockAnswer(index);
    }

    public void FireRevealContestantAnswer(int teamIndex, int lockedAnswerIndex)
    {
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.RevealAnswer();
        interf.RevealAnswer(teamIndex);
    }

    public void CalculateAnswerTime(float maxTime, float correctAnswerTime, int teamIndex)
    {
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        copy.answerTime += maxTime - correctAnswerTime;
        Moderator.Teams[teamIndex] = copy;
    }

    public void ResetTimer(int teamIndex)
    {
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.ResetTimer();
        interf.ResetTimer();
    }

    public void TimeElapsed()
    {
        Moderator.TimeElapsed();
    }

    #endregion

    #region AUDIENCE QUESTIONS

    public void AskQuestionToAudience(Question question)
    {
        audienceWinners = new List<AudienceBehavior>();
        audienceQuestion = question;
        foreach (AudienceBehavior audienceMember in audienceMembers)
        {
            audienceMember.AskQuestion(question);
        }
    }

    public void AddAudienceAnswer(int index, AudienceBehavior audienceMember)
    {
        if (index < 0 || index > 3) { return; }
        if (audienceQuestion.options[index].is_correct == 0) { return; }
        if (audienceWinners.Count < 3)
        {
            audienceWinners.Add(audienceMember);
        }
    }

    public void RevealAudienceAnswer()
    {
        foreach (AudienceBehavior audienceMember in audienceMembers)
        {
            if (audienceWinners.Contains(audienceMember))
            {
                audienceMember.ShowCongratulationsMessage();
            }
        }
        interf.ShowAudienceAnswer(audienceWinners);
    }

    #endregion

    #region LIFELINES

    public void RequestExpertLifeline()
    {
        Moderator.RequestExpertLifeline();
        interf.OnLifelineRequested();
    }

    public void RequestAudienceLifeline()
    {
        Moderator.RequestAudienceLifeline();
        interf.OnLifelineRequested();
    }

    public void RequestSwapLifeline()
    {
        print("TEST");
        Moderator.RequestSwapLifeline();
        interf.OnLifelineRequested();
    }

    public void RequestHalfLifeline()
    {
        Moderator.RequestHalfLifeline();
        interf.OnLifelineRequested();
    }

    public void AcceptExpertLifeline(int teamIndex)
    {
        // Create a copy of the team for the synclist to be updated
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        copy.expertLifeline = false;
        // Update synclist
        Moderator.Teams[teamIndex] = copy;
        // Apply lifeline
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.ApplyExpertLifeline();
        interf.ApplyExpertLifeline();
    }

    public void AcceptAudienceLifeline(int teamIndex, Question question)
    {
        // Create a copy of the team for the synclist to be updated
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        copy.audienceLifeline = false;
        // Update synclist
        Moderator.Teams[teamIndex] = copy;
        // Initialize poll result
        audiencePollResults = new List<int>();
        // Apply lifeline
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.ApplyAudienceLifeline();
        interf.ApplyAudienceLifeline();
        StartCoroutine(StartPollCountdown(90));
        foreach (AudienceBehavior audienceMember in audienceMembers)
            audienceMember.AskLifelineQuestion(question);
    }

    private IEnumerator StartPollCountdown(float countdown)
    {
        pollInProgress = true;
        yield return new WaitForSeconds(countdown);
        if (pollInProgress)
        {
            Moderator.ShowRevealPollResultRequest();
            pollInProgress = false;
        }
    }

    public void AcceptHalfLifeline(int teamIndex, List<int> indexesToRemove)
    {
        // Create a copy of the team for the synclist to be updated
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        copy.halfLifeline = false;
        // Update synclist
        Moderator.Teams[teamIndex] = copy;
        // Apply lifeline
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.ApplyHalfLifeline(indexesToRemove);
        interf.ApplyHalfLifeline(indexesToRemove);
    }

    public void AddAudiencePollAnswerIndex(int index)
    {
        audiencePollResults.Add(index);
        // When all audience members have answered
        if (audiencePollResults.Count == audienceMembers.Count && pollInProgress)
        {
            Moderator.ShowRevealPollResultRequest();
            pollInProgress = false;
        }
    }

    public void RevealPollResult(int teamIndex)
    {
        ContestantBehavior contestant = Contestants[Moderator.Teams[teamIndex].id];
        contestant.RevealPollResult(audiencePollResults);
        interf.RevealPollResult(audiencePollResults);
    }

    #endregion

    #region POINTS, LEADERBOARDS, WINNERS

    public void SetPoints(int teamIndex, int points)
    {
        Team copy = Team.CreateCopyFrom(Moderator.Teams[teamIndex]);
        copy.points = points;
        Moderator.Teams[teamIndex] = copy;
        interf.UpdateTeamDetails(Moderator.Teams);
    }

    public void ShowLeaderboard()
    {
        List<Team> teams = new List<Team>();
        foreach (Team team in Moderator.Teams)
        {
            teams.Add(team);
        }
        teams.Sort((a, b) => b.points.CompareTo(a.points));
        interf.ShowLeaderboard(teams);
    }

    public void HideLeaderboard()
    {
        interf.HideLeaderboard();
    }

    public void SetWinner(int winnerIndex)
    {
        Team winner = Moderator.Teams[winnerIndex];
        ContestantBehavior contestant = Contestants[Moderator.Teams[winnerIndex].id];
        contestant.ShowWinnerScreen();
        interf.ShowWinner(winner);
    }

    #endregion
}
