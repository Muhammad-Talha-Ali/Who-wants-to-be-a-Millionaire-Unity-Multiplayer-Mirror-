using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Mirror;

public class ContestantInterface : BaseInterface
{
    #region PARAMETERS

    [Header("Screens")]
    [SerializeField] private GameObject connectionScreen;
    [SerializeField] private GameObject questionScreen;
    [SerializeField] private GameObject disconnectScreen;
    [SerializeField] private GameObject winnerScreen;

    [Header("Connection Inputs")]
    [SerializeField] private TMP_InputField serverAddress;
    [SerializeField] private TMP_InputField teamCode;

    [Header("Question UI")]
    [SerializeField] private QuestionForm questionForm;
    [SerializeField] private GameObject answerConfirmationWindow;
    [SerializeField] private Transform questionTeamsContainer;
    [SerializeField] private QuestionTeams questionTeamPrefab;
    [SerializeField] private Image currentQuestionTeamLogo;
    [SerializeField] private TMP_Text currentQuestionTeamName;
    [SerializeField] private TMP_Text currentQuestionTeamScore;
    [SerializeField] private TMP_Text currentQuestionTeamQuestions;


    #endregion

    #region CACHES

    private ContestantNetworkManager networkManager;
    [HideInInspector]
    private ContestantBehavior networkBehavior;
    public ContestantBehavior NetworkBehavior
    {
        get
        {
            if (networkBehavior)
            {
                return networkBehavior;
            }
            else
            {
                ContestantBehavior[] behaviors = FindObjectsOfType<ContestantBehavior>();
                foreach (ContestantBehavior behavior in behaviors)
                {
                    if (behavior.hasAuthority)
                    {
                        networkBehavior = behavior;
                        return behavior;
                    }
                }
                return null;
            }
        }
        set
        {
            networkBehavior = value;
        }
    }

    #endregion

    #region STATES

    private Question currentQuestion;
    private bool canAnswer = true;
    private int answerIndex = -1;

    private Dictionary<int, Sprite> teamsLogos = new Dictionary<int, Sprite>();

    #endregion

    protected override void Awake()
    {
        base.Awake();
        networkManager = FindObjectOfType<ContestantNetworkManager>();
    }

    protected override void Start()
    {
        base.Start();
        // Load last working address & code combination (in case of disconnection)
        string networkAddress = PlayerPrefs.GetString("Address");
        string code = PlayerPrefs.GetString("Code");
        if (!string.IsNullOrEmpty(networkAddress))
            serverAddress.text = networkAddress;
        if (!string.IsNullOrEmpty(code))
            teamCode.text = code;
    }

    public void Connect()
    {
        networkManager.networkAddress = serverAddress.text;
        networkManager.Code = teamCode.text;
        if (!NetworkClient.active)
        {
            networkManager.StartClient();
        }
        else
        {
            NetworkClient.Connect(networkManager.networkAddress);
        }
    }

    public void Initialize()
    {
        PlayerPrefs.SetString("Address", networkManager.networkAddress);
        PlayerPrefs.SetString("Code", networkManager.Code);
        disconnectScreen.SetActive(false);
        connectionScreen.SetActive(false);
    }

    public void SetLogoData(int teamId, byte[] logoData)
    {
        print($"Set logo data {teamId}");
        if (!teamsLogos.ContainsKey(teamId))
        {
            Texture2D texture = new Texture2D(150, 150);
            texture.LoadImage(logoData);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
            teamsLogos.Add(teamId, sprite);
        }
    }

    public void ShowDisconnectedScreen()
    {
        disconnectScreen.SetActive(true);
    }

    public IEnumerator HideDisconnectedScreen(float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);
        disconnectScreen.SetActive(false);
    }

    public void ShowWinnerScreen()
    {
        winnerScreen.SetActive(true);
    }

    #region Questions

    public void ShowQuestion(Question question, Team team, List<Team> teams)
    {
        currentQuestion = question;
        questionScreen.SetActive(true);
        questionForm.SetQuestion(question.question, team);

        UpdateTeams(teams, team);
    }

    internal void UpdateTeams(List<Team> teams, Team team)
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
                currentQuestionTeamLogo.sprite = teamsLogos[tmp.id];
                currentQuestionTeamName.text = tmp.name;
                currentQuestionTeamScore.text = tmp.points.ToString();
                currentQuestionTeamQuestions.text = $"{tmp.questionsNumber}\n<size=5> OUT OF </size>\n10";
            }
            else
            {
                QuestionTeams questionTeam = Instantiate(questionTeamPrefab, questionTeamsContainer);
                questionTeam.Logo.sprite = teamsLogos[tmp.id];
                questionTeam.Name.text = tmp.name;
                questionTeam.Score.text = tmp.points.ToString();
                questionTeam.Questions.text = $"{tmp.questionsNumber}\n<size=5> OUT OF </size>\n10";
            }
        }
    }

    public void ShowOptions()
    {
        questionScreen.GetComponent<QuestionPanel>().ShowOptions();
        questionForm.ShowOptions(currentQuestion.options);
        canAnswer = true;
    }

    public void HideQuestionScreen()
    {
        questionScreen.SetActive(false);
    }

    #endregion

    #region Answers

    public void ChooseAnswer(int index)
    {
        if (!canAnswer)
            return;
        answerIndex = index;
        answerConfirmationWindow.SetActive(true);
        questionForm.StopTimer();
    }

    public void LockAnswer()
    {
        NetworkBehavior.LockAnswer(answerIndex);
        questionForm.SetButtonLocked(answerIndex);
        questionForm.StopTimer();
    }

    public void RevealAnswer()
    {
        if (currentQuestion.options[answerIndex].is_correct == 1)
        {
            questionForm.SetButtonCorrect(answerIndex);
        }
        else
        {
            questionForm.SetButtonWrong(answerIndex);
        }
    }

    #endregion

    #region Timer

    public void SetTimer(float time)
    {
        questionForm.SetTimer(time);
    }

    public void ResetTimer()
    {
        questionForm.ResetTimer();
    }

    public void TimeElapsed()
    {
        NetworkBehavior.TimeElapsed();
    }

    #endregion

    #region Lifeline Requests

    public void RequestExpertLifeline()
    {
        NetworkBehavior.RequestExpertLifeline();
        OnLifelineRequested();
    }

    public void RequestAudienceLifeline()
    {
        NetworkBehavior.RequestAudienceLifeline();
        OnLifelineRequested();
    }

    public void RequestSwapLifeline()
    {
        NetworkBehavior.RequestSwapLifeline();
        OnLifelineRequested();
    }

    public void RequestHalfLifeline()
    {
        NetworkBehavior.RequestHalfLifeline();
        OnLifelineRequested();
    }

    private void OnLifelineRequested()
    {
        questionForm.StopTimer();
        canAnswer = false;
    }

    #endregion

    #region Lifeline Applications

    public void ApplyExpertLifeline()
    {
        questionForm.StartLifetimeTimer(90);
        questionForm.DisableExpertLifeline();
        canAnswer = true;
    }

    public void ApplyAudienceLifeline()
    {
        questionForm.StartLifetimeTimer(90);
        questionForm.DisableAudienceLifeline();
        canAnswer = true;
    }

    public void ApplyHalfLifeline(List<int> indexesToRemove)
    {
        questionForm.DisableOptions(indexesToRemove);
        questionForm.DisableHalfLifeline();
        questionForm.StartTimer();
        canAnswer = true;
    }

    public void RevealPollResult(List<int> audiencePollResults)
    {
        for (int i = 0; i < 4; i++)
        {
            questionForm.SetPollResult(i, audiencePollResults.Count(x => x == i));
        }
        questionForm.StartTimer();
    }

    #endregion
}
