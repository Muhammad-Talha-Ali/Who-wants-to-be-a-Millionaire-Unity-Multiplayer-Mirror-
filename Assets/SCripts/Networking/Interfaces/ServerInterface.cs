using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class ServerInterface : BaseInterface
{
    #region PARAMETERS

    [Header("Screens")]
    [SerializeField] private GameObject teamConnectionScreen;
    [SerializeField] private GameObject audienceConnectionScreen;
    [SerializeField] private GameObject questionScreen;
    [SerializeField] private GameObject audienceAnswerScreen;
    [SerializeField] private GameObject teamsScreen;
    [SerializeField] private GameObject leaderboardScreen;
    [SerializeField] private GameObject winnerScreen;

    [Header("Teams Connection")]
    [SerializeField] private Transform teamConnectionContainer;
    [SerializeField] private TeamConnectionPanel teamConnectionPanelPrefab;

    [Header("Questions")]
    public QuestionForm QuestionForm;
    [SerializeField] private Transform questionTeamsContainer;
    [SerializeField] private QuestionTeams questionTeamPrefab;
    [SerializeField] private Image currentQuestionTeamLogo;
    [SerializeField] private TMP_Text currentQuestionTeamName;
    [SerializeField] private TMP_Text currentQuestionTeamScore;
    [SerializeField] private TMP_Text currentQuestionTeamQuestions;

    [Header("Teams Screen")]
    public Transform teamDetailsContainer;
    public TeamDetails teamDetailsPrefab;

    [Header("Leaderboard")]
    [SerializeField] private List<LeaderboardRanking> rankings;

    [Header("Audience Connectivity")]
    [SerializeField] private TMP_Text audienceCode;
    [SerializeField] private TMP_Text timerMinute1;
    [SerializeField] private TMP_Text timerMinute2;
    [SerializeField] private TMP_Text timerSecond1;
    [SerializeField] private TMP_Text timerSecond2;
    [SerializeField] private Transform audienceNamesContainer;
    [SerializeField] private GameObject audienceNamePrefab;

    [Header("Audience Question")]
    [SerializeField] private Transform audienceWinnersNameContainer;

    [Header("Contestant Winner")]
    [SerializeField] private Image winnerLogo;
    [SerializeField] private TMP_Text winnerName;
    [SerializeField] private TMP_Text winnerPoints;

    #endregion

    #region CACHES

    private ServerNetworkManager networkManager;

    #endregion

    #region STATES

    private TeamConnectionPanel[] teamConnectionPanels;
    [HideInInspector]
    public int CurrentTeamId;

    // Contestants questions
    private int answerIndex = -1;
    [HideInInspector]
    public Question CurrentQuestion;
    [HideInInspector]
    public bool QuestionFired = false;
    [HideInInspector]
    public bool OptionsFired = false;

    // Audience connectivity
    private float currentAudienceTimer;
    private bool audienceTimerOn = false;
    private List<string> audienceNames = new List<string>();

    #endregion

    protected override void Awake()
    {
        base.Awake();
        networkManager = FindObjectOfType<ServerNetworkManager>();
    }

    protected override void Start()
    {
        base.Start();
        networkManager.StartHost();
    }

    private void Update()
    {
        if (audienceTimerOn)
        {
            if (currentAudienceTimer <= 0)
            {
                audienceTimerOn = false;
                audienceConnectionScreen.SetActive(false);
                return;
            }
            int minute = (int)currentAudienceTimer / 60;
            int second = (int)currentAudienceTimer % 60;
            string minute1 = minute >= 10 ? (minute / 10).ToString() : "0";
            string minute2 = minute < 10 ? minute.ToString() : (minute % 10).ToString();
            string second1 = second >= 10 ? (second / 10).ToString() : "0";
            string second2 = second < 10 ? second.ToString() : (second % 10).ToString();

            timerMinute1.text = minute1;
            timerMinute2.text = minute2;
            timerSecond1.text = second1;
            timerSecond2.text = second2;
            currentAudienceTimer -= Time.deltaTime;
        }
    }

    public void TriggerTeamConnection(SyncList<Team> teams)
    {
        teamConnectionScreen.SetActive(true);

        teamConnectionPanels = new TeamConnectionPanel[teams.Count];
        // Create connection panel for each team
        int i = 0;
        foreach (Team selectedTeam in teams)
        {
            TeamConnectionPanel teamConnection = Instantiate(teamConnectionPanelPrefab, teamConnectionContainer);
            teamConnection.Name.text = selectedTeam.name;
            teamConnection.Logo.sprite = selectedTeam.logoSprite;
            StartCoroutine(teamConnection.LoadLogo(selectedTeam));
            // Generate / Retrieve code
            teamConnection.Code.text = $"Code: {selectedTeam.code}";
            teamConnectionPanels[i] = teamConnection;
            i++;
        }
    }

    public void ProceedToMatch(SyncList<Team> teams)
    {
        teamConnectionScreen.SetActive(false);
        ShowTeamsScreen(teams);
    }

    public void OnContestantConnected(int teamIndex)
    {
        teamConnectionPanels[teamIndex].SetConnected(true);
    }

    #region Teams Screen

    public void ShowTeamsScreen(SyncList<Team> teams)
    {
        teamsScreen.SetActive(true);
        foreach (Transform child in teamDetailsContainer)
            Destroy(child.gameObject);
        foreach (Team team in teams)
        {
            TeamDetails teamDetails = Instantiate(teamDetailsPrefab, teamDetailsContainer);
            teamDetails.Name.text = team.name;
            teamDetails.Logo.sprite = team.logoSprite;
            teamDetails.Points.text = team.points.ToString();
            teamDetails.QuestionNumbers.text = team.questionsNumber + "\n<size=5> OUT OF </size>\n10";
            teamDetails.Lifelines[0].interactable = team.expertLifeline;
            teamDetails.Lifelines[1].interactable = team.audienceLifeline;
            teamDetails.Lifelines[2].interactable = team.swapLifeline;
            teamDetails.Lifelines[3].interactable = team.halfLifeline;
        }
    }

    public void UpdateTeamDetails(SyncList<Team> teams)
    {
        if (!teamsScreen.activeSelf)
            return;
        foreach (Transform child in teamDetailsContainer)
            Destroy(child.gameObject);
        foreach (Team team in teams)
        {
            TeamDetails teamDetails = Instantiate(teamDetailsPrefab, teamDetailsContainer);
            teamDetails.Name.text = team.name;
            teamDetails.Logo.sprite = team.logoSprite;
            teamDetails.Points.text = team.points.ToString();
            teamDetails.QuestionNumbers.text = team.questionsNumber + "\n<size=5> OUT OF </size>\n10";
            teamDetails.Lifelines[0].interactable = team.expertLifeline;
            teamDetails.Lifelines[1].interactable = team.audienceLifeline;
            teamDetails.Lifelines[2].interactable = team.swapLifeline;
            teamDetails.Lifelines[3].interactable = team.halfLifeline;
        }
    }

    #endregion

    #region Contestant Questions

    public void ShowQuestion(Team team, Question question, SyncList<Team> teams)
    {
        teamsScreen.SetActive(false);
        questionScreen.SetActive(true);
        QuestionForm.SetQuestion(question.question, team);
        CurrentTeamId = team.id;
        CurrentQuestion = question;
        QuestionFired = true;

        UpdateTeams(teams, team);
    }

    internal void UpdateTeams(SyncList<Team> teams, Team team)
    {
        foreach (Transform child in questionTeamsContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Team tmp in teams)
        {
            if (tmp.id == team.id)
            {
                QuestionTeams questionTeam = Instantiate(questionTeamPrefab, questionTeamsContainer);
                questionTeam.Logo.sprite = tmp.logoSprite;
                questionTeam.Name.text = tmp.name;
                questionTeam.Score.text = tmp.points.ToString();
                questionTeam.Questions.text = $"{tmp.questionsNumber}\n<size=5> OUT OF </size>\n10";
                break;
            }
        }
        foreach (Team tmp in teams)
        {
            if (tmp.id == team.id)
            {
                currentQuestionTeamLogo.sprite = tmp.logoSprite;
                currentQuestionTeamName.text = tmp.name;
                currentQuestionTeamScore.text = tmp.points.ToString();
                currentQuestionTeamQuestions.text = $"{tmp.questionsNumber}\n<size=5> OUT OF </size>\n10";
            }
            else
            {
                QuestionTeams questionTeam = Instantiate(questionTeamPrefab, questionTeamsContainer);
                questionTeam.Logo.sprite = tmp.logoSprite;
                questionTeam.Name.text = tmp.name;
                questionTeam.Score.text = tmp.points.ToString();
                questionTeam.Questions.text = $"{tmp.questionsNumber}\n<size=5> OUT OF </size>\n10";
            }
        }
    }

    public void ShowOptions()
    {
        questionScreen.GetComponent<QuestionPanel>().ShowOptions();
        QuestionForm.ShowOptions(CurrentQuestion.options);
        OptionsFired = true;
    }

    public void HideQuestionScreen()
    {
        questionScreen.SetActive(false);
        ShowTeamsScreen(networkManager.Moderator.Teams);
    }

    public void ResetTimer()
    {
        QuestionForm.ResetTimer();
    }

    #endregion

    #region Contestant Answers

    public void LockAnswer(int index)
    {
        answerIndex = index;
        QuestionForm.SetButtonLocked(index);
        QuestionForm.StopTimer();
        QuestionFired = false;
        OptionsFired = false;
    }

    public void RevealAnswer(int teamIndex)
    {
        if (CurrentQuestion.options[answerIndex].is_correct == 1)
        {
            QuestionForm.SetButtonCorrect(answerIndex);
            networkManager.CalculateAnswerTime(180f, QuestionForm.GetTimer(), teamIndex);
        }
        else
        {
            QuestionForm.SetButtonWrong(answerIndex);
        }
    }

    #endregion

    #region Leaderboard

    public void ShowLeaderboard(List<Team> teams)
    {
        leaderboardScreen.SetActive(true);
        ResetRankings();
        for (int i = 0; i < teams.Count; i++)
        {
            Team team = teams[i];
            LeaderboardRanking ranking = rankings[i];
            ranking.Logo.sprite = team.logoSprite;
            ranking.Name.text = team.name;
            ranking.Points.text = $"{team.points} POINTS";
            ranking.Time.text = $"{Mathf.CeilToInt(team.answerTime / 60)} MINS";
            ranking.gameObject.SetActive(true);
        }
    }

    public void HideLeaderboard()
    {
        leaderboardScreen.SetActive(false);
    }

    public void ResetRankings()
    {
        foreach (LeaderboardRanking ranking in rankings)
        {
            ranking.Name.text = "";
            ranking.Points.text = "";
            ranking.Time.text = "";
            ranking.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Lifelines

    public void OnLifelineRequested()
    {
        QuestionForm.StopTimer();
    }

    public void ApplyExpertLifeline()
    {
        QuestionForm.StartLifetimeTimer(90);
        QuestionForm.DisableExpertLifeline();
    }

    public void ApplyAudienceLifeline()
    {
        QuestionForm.StartLifetimeTimer(90);
        QuestionForm.DisableAudienceLifeline();
    }

    public void ApplyHalfLifeline(List<int> indexesToRemove)
    {
        QuestionForm.DisableOptions(indexesToRemove);
        QuestionForm.DisableHalfLifeline();
        QuestionForm.StartTimer();
    }

    public void RevealPollResult(List<int> audiencePollResults)
    {
        for (int i = 0; i < 4; i++)
        {
            QuestionForm.SetPollResult(i, audiencePollResults.Count<int>(x => x == i));
        }
        QuestionForm.StartTimer();
    }

    #endregion

    #region Audience

    public void StartAudienceConnection(string code)
    {
        audienceConnectionScreen.SetActive(true);
        audienceCode.text = code.ToString();
        currentAudienceTimer = ModeratorInterface.AudienceConnectionTimer;
        StartAudienceConnectionTimer();
    }

    private void StartAudienceConnectionTimer()
    {
        audienceTimerOn = true;
    }

    public void AddAudienceMember(string audienceMemberName)
    {
        audienceNames.Add(audienceMemberName);
        GameObject audienceName = Instantiate(audienceNamePrefab, audienceNamesContainer);
        audienceName.GetComponentInChildren<TMP_Text>().text = audienceMemberName;
    }

    public void ShowAudienceAnswer(List<AudienceBehavior> audienceWinners)
    {
        teamsScreen.SetActive(false);
        audienceAnswerScreen.SetActive(true);
        foreach(Transform child in audienceWinnersNameContainer)
            Destroy(child.gameObject);
        foreach(AudienceBehavior audienceWinner in audienceWinners)
        {
            GameObject audienceName = Instantiate(audienceNamePrefab, audienceWinnersNameContainer);
            audienceName.GetComponentInChildren<TMP_Text>().text = audienceWinner.Name;
        }
        StartCoroutine(HideAudienceResults(3));
    }

    IEnumerator HideAudienceResults(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        audienceAnswerScreen.SetActive(false);
        teamsScreen.SetActive(true);
    }

    #endregion

    #region Contestant Winner

    public void ShowWinner(Team team)
    {
        teamsScreen.SetActive(false);
        winnerScreen.SetActive(true);
        winnerLogo.sprite = team.logoSprite;
        winnerName.text = team.name;
        winnerPoints.text = team.points + " POINTS";
    }

    #endregion
}
