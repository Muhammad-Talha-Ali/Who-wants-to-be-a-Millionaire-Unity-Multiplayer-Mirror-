using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Mirror;

public class ModeratorInterface : BaseInterface
{
    #region PARAMETERS

    [Header("Screens")]
    [SerializeField] private GameObject connectionScreen;
    [SerializeField] private GameObject matchSelectionScreen;
    [SerializeField] private GameObject audienceConnectionScreen;
    [SerializeField] private GameObject audienceTimerCompleteScreen;
    [SerializeField] private GameObject teamSelectionScreen;
    [SerializeField] private GameObject teamConnectionScreen;
    [SerializeField] private GameObject mainScreen;

    [Header("Match Selection")]
    [SerializeField] private MatchSelectionButton matchSelectionButton;
    [SerializeField] private Transform matchSelectionContainer;
    [SerializeField] private Image matchTeamImagePrefab;

    [Header("Team Selection")]
    public Transform TeamSelectionContainer;
    public TeamSelectionButton TeamSelectionButton;
    public Button ProceedToTeamConnectionButton;

    [Header("Team Connection")]
    public Transform TeamConnectionContainer;
    public TeamConnectionPanel TeamConnectionPanel;
    public Button ProceedToMatchButton;

    [Header("Team Controls")]
    public Transform TeamControlsContainer;
    public TeamControlButton TeamControlButton;

    [Header("Confirmation Panels")]
    public GameObject FireQuestionConfirmationPanel;
    public GameObject ResetTimerConfirmationPanel;
    public GameObject RefireQuestionConfirmationPanel;
    public GameObject WinnerConfirmationPanel;
    public GameObject AdjustPointConfirmationPanel;
    public GameObject AskAudienceConfirmationPanel;
    public GameObject LifelineConfirmation;

    [Header("Option Panels")]
    public GameObject RevealOptionsPanel;
    public GameObject RevealAnswerPanel;
    public GameObject RevealAudienceAnswerPanel;

    [Header("Audience Connection")]
    public static int AudienceConnectionTimer = 80;
    public TMP_Text AudienceCode;
    public Button StartAudienceConnectionButton;
    public GameObject AudienceConnectionPanel;
    public TMP_Text TimerMinute1;
    public TMP_Text TimerMinute2;
    public TMP_Text TimerSecond1;
    public TMP_Text TimerSecond2;
    public Transform AudienceNamesContainer;
    public GameObject AudienceNamePrefab;
    public TMP_Text AudienceConnectionResult;

    [Header("Question Controls")]
    public Button FireQuestionButton;
    public Button ResetTimerButton;
    public Button RefireQuestionButton;

    [Header("Point Adjustment Controls")]
    public TMP_Dropdown TeamSelectDropdown;
    public Image PointsTeamLogo;
    public TMP_Text PointsTeamName;
    public TMP_Text PointsTeamValue;
    public TMP_InputField PointsValue;

    [Header("Announcement")]
    public GameObject AnnouncementPanel;
    public GameObject WinnerSelectionPanel;

    [Header("Winner Selection Controls")]
    public TMP_Dropdown WinnerSelectDropdown;
    public Image WinnerLogo;
    public TMP_Text WinnerName;
    public TMP_Text WinnerPoints;

    [Header("Lifelines")]
    public Image LifelineConfirmationImage;
    public Sprite ExpertSprite;
    public Sprite AudienceSprite;
    public Sprite SwapSprite;
    public Sprite HalfSprite;
    public TMP_Text LifelineText;
    public Button AcceptLifelineRequestButton;

    [Header("Audience Poll")]
    public GameObject RevealPollResultPanel;

    [Header("Disconnection")]
    public GameObject DisconnectionWindow;
    public TMP_Text DisconnectionText;

    #endregion

    #region CACHES

    private ModeratorNetworkManager networkManager;
    [HideInInspector]
    public ModeratorBehavior NetworkBehavior;

    #endregion

    #region STATES

    // Match Selection
    private int currentMatchIndex;
    private List<MatchSelectionButton> selectionButtons = new List<MatchSelectionButton>();
    private Match[] matches;
    private Match currentMatch;

    // Teams Selection
    private List<Team> selectedTeams = new List<Team>();

    // Teams Connection
    private TeamConnectionPanel[] teamConnectionPanels;

    // Teams Controls
    private TeamControlButton[] controlButtons;
    private int lastSelectedTeamIndex = -1;
    private int currentSelectedTeamIndex = -1;

    // Contestant Questions
    private bool questionInProgress = false;
    private Question questionFired;

    // Contestant Answer
    private int lockedAnswerIndex = -1;

    // Audience Connection
    private float currentAudienceTimer;
    private bool audienceTimerOn = false;
    private List<string> audienceNames = new List<string>();

    // Point Adjustment
    private int currentSelectedPointAdjustmentIndex = -1;

    // Winner Selection
    private int currentSelectedWinnerIndex = -1;

    // Leaderboard
    private bool leaderboardShown = false;

    // Controls
    private int controlActionIndex;

    private List<int> requestedLogoIds = new List<int>();
    private Dictionary<int, byte[]> teamsLogos = new Dictionary<int, byte[]>();

    #endregion

    protected override void Awake()
    {
        base.Awake();
        networkManager = FindObjectOfType<ModeratorNetworkManager>();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void Connect(TMP_InputField address)
    {
        networkManager.networkAddress = address.text;
        networkManager.StartClient();
    }

    public async void Initialize(Match[] matches)
    {
        this.matches = matches;

        foreach(Match match in matches)
        {
            foreach(Team team in match.teams)
            {
                if (requestedLogoIds.Contains(team.id)) { continue; }
                NetworkBehavior.RequestLogo(team.id);
            }
        }
        StartCoroutine(ShowMainScreen(3));
        //ShowMainScreen();
    }

    public void SetLogoData(int teamId, byte[] logoData)
    {
        if (!teamsLogos.ContainsKey(teamId))
        {
            teamsLogos.Add(teamId, logoData);
        }
    }

    public IEnumerator ShowMainScreen(float waitTime = 0)
    {
        yield return new WaitForSeconds(waitTime);

        connectionScreen.SetActive(false);
        matchSelectionScreen.SetActive(true);

        foreach (Match match in matches)
        {
            MatchSelectionButton matchButton = Instantiate(matchSelectionButton, matchSelectionContainer);
            matchButton.Date.text = match.event_date;
            //StartCoroutine(LoadLogos(matchButton, match));

            foreach (Team team in match.teams)
            {
                Texture2D texture = new Texture2D(150, 150);
                texture.LoadImage(teamsLogos[team.id]);
                team.logoSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
                Image teamImage = Instantiate(matchTeamImagePrefab, matchButton.TeamImagesContainer);
                Image image = teamImage.transform.GetComponentsInChildren<Image>()[1];
                image.sprite = team.logoSprite;
            }

            matchButton.GetComponent<Animator>().enabled = false;
            selectionButtons.Add(matchButton);
            matchButton.gameObject.SetActive(false);
        }
        if(selectionButtons.Count > 0)
        {
            MatchSelectionButton middle = selectionButtons[0];
            middle.gameObject.SetActive(true);
            middle.transform.localScale = Vector3.one;
            currentMatchIndex = 0;
            currentMatch = matches[currentMatchIndex];
        }
        if (selectionButtons.Count > 1)
        {
            MatchSelectionButton right = selectionButtons[1];
            right.gameObject.SetActive(true);
            right.GetComponent<RectTransform>().anchoredPosition = Vector3.right * 350;
            right.GetComponent<RectTransform>().localScale = Vector3.one * 0.6f;
        }
        if (selectionButtons.Count > 2)
        {
            MatchSelectionButton left = selectionButtons[selectionButtons.Count - 1];
            left.gameObject.SetActive(true);
            left.GetComponent<RectTransform>().anchoredPosition = Vector3.right * -350;
            left.GetComponent<RectTransform>().localScale = Vector3.one * 0.6f;
        }
    }

    public void SelectNextMatch()
    {
        foreach(MatchSelectionButton button in selectionButtons)
        {
            button.GetComponent<Animator>().enabled = true;
            button.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            button.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        if (selectionButtons.Count < 2) { return; }
        else if (selectionButtons.Count == 2)
        {
            if(currentMatchIndex != 0) { return; }
            currentMatchIndex = 1;
            selectionButtons[0].GetComponent<Animator>().Play("Center To Left");
            selectionButtons[1].GetComponent<Animator>().Play("Right To Center");
        }
        else
        {
            int currentLeftIndex = currentMatchIndex == 0 ? matches.Length - 1 : currentMatchIndex - 1;
            int currentRightIndex = currentMatchIndex == matches.Length - 1 ? 0 : currentMatchIndex + 1;

            selectionButtons[currentLeftIndex].GetComponent<Animator>().Play("Left To Fade");
            selectionButtons[currentMatchIndex].GetComponent<Animator>().Play("Center To Left");
            selectionButtons[currentRightIndex].GetComponent<Animator>().Play("Right To Center");

            currentMatchIndex = currentRightIndex;
            currentRightIndex = currentMatchIndex == matches.Length - 1 ? 0 : currentMatchIndex + 1;
            selectionButtons[currentRightIndex].gameObject.SetActive(true);
            selectionButtons[currentRightIndex].GetComponent<Animator>().Play("Fade To Right");
        }
        currentMatch = matches[currentMatchIndex];
    }

    public void SelectPreviousMatch()
    {
        foreach (MatchSelectionButton button in selectionButtons)
        {
            button.GetComponent<Animator>().enabled = true;
            button.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            button.GetComponent<RectTransform>().localScale = Vector3.one;
        }
        if (selectionButtons.Count < 2) { return; }
        else if (selectionButtons.Count == 2)
        {
            if (currentMatchIndex != 1) { return; }
            currentMatchIndex = 0;
            selectionButtons[0].GetComponent<Animator>().Play("Left To Center");
            selectionButtons[1].GetComponent<Animator>().Play("Center To Right");
        }
        else
        {
            int currentLeftIndex = currentMatchIndex == 0 ? matches.Length - 1 : currentMatchIndex - 1;
            int currentRightIndex = currentMatchIndex == matches.Length - 1 ? 0 : currentMatchIndex + 1;

            selectionButtons[currentLeftIndex].GetComponent<Animator>().Play("Left To Center");
            selectionButtons[currentMatchIndex].GetComponent<Animator>().Play("Center To Right");
            selectionButtons[currentRightIndex].GetComponent<Animator>().Play("Right To Fade");

            currentMatchIndex = currentLeftIndex;
            currentLeftIndex = currentMatchIndex == 0 ? matches.Length - 1 : currentMatchIndex - 1;
            selectionButtons[currentLeftIndex].gameObject.SetActive(true);  
            selectionButtons[currentLeftIndex].GetComponent<Animator>().Play("Fade To Left");
        }
        currentMatch = matches[currentMatchIndex];
    }

    IEnumerator LoadLogos(MatchSelectionButton matchButton, Match match)
    {
        yield return StartCoroutine(ApiManager.Instance.LoadTeamLogos(match.teams));
        foreach(Team team in match.teams)
        {
            Image teamImage = Instantiate(matchTeamImagePrefab, matchButton.TeamImagesContainer);
            Image image = teamImage.transform.GetComponentsInChildren<Image>()[1];
            image.sprite = team.logoSprite;
        }
    }

    public void ProceedToAudienceConnection()
    {
        matchSelectionScreen.SetActive(false);
        audienceConnectionScreen.SetActive(true);
    }

    public void GetCurrentMatchDatas()
    {
        NetworkBehavior.GetCurrentMatchDatas(currentMatchIndex);
    }

    public void OpenAudienceConnection()
    {
        int code = GenerateConnectionCode();
        AudienceCode.text = code.ToString();
        NetworkBehavior.SetAudienceCode(code.ToString(), true);

        StartAudienceConnectionButton.gameObject.SetActive(false);
        AudienceConnectionPanel.SetActive(true);

        currentAudienceTimer = AudienceConnectionTimer;
        StartAudienceConnectionTimer();
    }

    private void StartAudienceConnectionTimer()
    {
        audienceTimerOn = true;
    }

    private void Update()
    {
        if (audienceTimerOn)
        {
            if(currentAudienceTimer <= 0)
            {
                audienceTimerOn = false;
                ShowAudienceTimerComplete();
                return;
            }
            int minute = (int)currentAudienceTimer / 60;
            int second = (int)currentAudienceTimer % 60;
            string minute1 = minute >= 10 ? (minute / 10).ToString() : "0";
            string minute2 = minute < 10 ? minute.ToString() : (minute % 10).ToString();
            string second1 = second >= 10 ? (second / 10).ToString() : "0";
            string second2 = second < 10 ? second.ToString() : (second % 10).ToString();

            TimerMinute1.text = minute1;
            TimerMinute2.text = minute2;
            TimerSecond1.text = second1;
            TimerSecond2.text = second2;
            currentAudienceTimer -= Time.deltaTime;
        }
    }

    private void ShowAudienceTimerComplete()
    {
        audienceConnectionScreen.SetActive(false);
        audienceTimerCompleteScreen.SetActive(true);
        AudienceConnectionResult.text = audienceNames.Count + " MEMBERS HAVE CONNECTED";
    }

    public void AddAudienceMember(string audienceMemberName)
    {
        audienceNames.Add(audienceMemberName);
        GameObject audienceName = Instantiate(AudienceNamePrefab, AudienceNamesContainer);
        audienceName.GetComponentInChildren<TMP_Text>().text = audienceMemberName;
    }

    public void ProceedToTeamSelection()
    {

        //currentMatch = await ApiManager.Instance.GetCurrentMatchDatas(currentMatch.id);

        audienceTimerCompleteScreen.SetActive(false);
        teamSelectionScreen.SetActive(true);
        foreach (Team team in NetworkBehavior.CurrentMatch.teams)
        {
            TeamSelectionButton TeamButton = Instantiate(TeamSelectionButton, TeamSelectionContainer);
            TeamButton.Name.text = team.name;
            TeamButton.SelectedName.text = team.name;
            TeamButton.Team = team;
            Texture2D texture = new Texture2D(150, 150);
            texture.LoadImage(teamsLogos[team.id]);
            team.logoSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2));
            TeamButton.Logo.sprite = team.logoSprite;
            StartCoroutine(TeamButton.LoadLogo(team));
            TeamButton.button.onClick.AddListener(() =>
            {
                if (selectedTeams.Contains(team))
                    selectedTeams.Remove(team);
                else
                    selectedTeams.Add(team);
                CheckTeamConnectionPossibility();
            });
        }
    }

    private void CheckTeamConnectionPossibility()
    {
        ProceedToTeamConnectionButton.interactable = selectedTeams.Count > 1;
    }

    public void ProceedToTeamConnection()
    {
        teamSelectionScreen.SetActive(false);
        teamConnectionScreen.SetActive(true);

        teamConnectionPanels = new TeamConnectionPanel[selectedTeams.Count];
        int i = 0;
        foreach (Team selectedTeam in selectedTeams)
        {
            selectedTeam.code = GenerateConnectionCode();

            TeamConnectionPanel teamConnection = Instantiate(TeamConnectionPanel, TeamConnectionContainer);
            teamConnection.Name.text = selectedTeam.name;
            teamConnection.Logo.sprite = selectedTeam.logoSprite;
            // Generate / Retrieve code
            teamConnection.Code.text = string.Concat("Code: ", selectedTeam.code);

            teamConnectionPanels[i] = teamConnection;
            i++;
        }

        NetworkBehavior.TriggerTeamConnection(selectedTeams);
    }

    public void ProceedToMatch()
    {
        NetworkBehavior.ProceedToMatch();
        teamConnectionScreen.SetActive(false);
        mainScreen.SetActive(true);

        controlButtons = new TeamControlButton[NetworkBehavior.Teams.Count];

        List<string> teamNames = new List<string>();
        int i = 0;
        foreach (Team team in NetworkBehavior.Teams)
        {
            TeamControlButton teamButton = Instantiate(TeamControlButton, TeamControlsContainer);
            teamButton.Name.text = team.name;
            teamButton.SelectedName.text = team.name;
            teamButton.Points.text = "POINTS: 0";
            teamButton.SelectedPoints.text = "POINTS: 0";
            teamButton.Questions.text = "0\n<size=5> OUT OF </size>\n10";
            teamButton.SelectedQuestions.text = "0\n<size=5> OUT OF </size>\n10";
            //teamButton.Logo.sprite = currentMatch.teams[i].logoSprite;

            int index = i;
            teamButton.button.onClick.AddListener(() =>
            {
                currentSelectedTeamIndex = index;
            });
            controlButtons[i] = teamButton;
            teamNames.Add(team.name);
            i++;
        }
        // Points adjustment options
        TeamSelectDropdown.AddOptions(teamNames);
        TeamSelectDropdown.onValueChanged.AddListener((int index) =>
        {
            currentSelectedPointAdjustmentIndex = index - 1;
            if (currentSelectedPointAdjustmentIndex == -1)
                return;
            PointsTeamLogo.sprite = NetworkBehavior.CurrentMatch.teams[currentSelectedPointAdjustmentIndex].logoSprite;
            PointsTeamName.text = NetworkBehavior.Teams[currentSelectedPointAdjustmentIndex].name;
            PointsTeamValue.text = NetworkBehavior.Teams[currentSelectedPointAdjustmentIndex].points.ToString();
        });
        // Winner selection options
        WinnerSelectDropdown.AddOptions(teamNames);
        WinnerSelectDropdown.onValueChanged.AddListener((int index) =>
        {
            currentSelectedWinnerIndex = index - 1;
            if (currentSelectedWinnerIndex == -1)
                return;
            WinnerLogo.sprite = NetworkBehavior.CurrentMatch.teams[currentSelectedWinnerIndex].logoSprite;
            WinnerName.text = NetworkBehavior.Teams[currentSelectedWinnerIndex].name;
            WinnerPoints.text = NetworkBehavior.Teams[currentSelectedWinnerIndex].points.ToString();
        });
    }

    public void ShowWinnerConfirmation()
    {
        WinnerConfirmationPanel.SetActive(true);
    }

    public void ShowAdjustPointConfirmation()
    {
        AdjustPointConfirmationPanel.SetActive(true);
    }

    public void ShowAskAudienceConfirmation()
    {
        AskAudienceConfirmationPanel.SetActive(true);
    }

    public void SelectWinner()
    {
        NetworkBehavior.SetWinner(currentSelectedWinnerIndex);
    }

    public void AdujstPoint()
    {
        int points = NetworkBehavior.Teams[currentSelectedPointAdjustmentIndex].points;
        int.TryParse(PointsValue.text, out points);
        NetworkBehavior.SetPoints(currentSelectedPointAdjustmentIndex, points);
        controlButtons[currentSelectedPointAdjustmentIndex].SetPoints(points);
        ResetPointAdjustment();
    }

    public void ResetWinnerSelection()
    {
        currentSelectedWinnerIndex = -1;
        WinnerSelectDropdown.value = 0;
        WinnerLogo.sprite = default;
        WinnerName.text = default;
        WinnerPoints.text = default;
    }

    public void ResetPointAdjustment()
    {
        currentSelectedPointAdjustmentIndex = -1;
        TeamSelectDropdown.value = 0;
        PointsTeamLogo.sprite = default;
        PointsTeamName.text = default;
        PointsTeamValue.text = default;
        PointsValue.text = default;
    }

    private int GenerateConnectionCode()
    {
        return Random.Range(100000, 999999);
    }

    public void OnContestantConnected(int teamIndex)
    {
        teamConnectionPanels[teamIndex].SetConnected(true);
        teamConnectionPanels[teamIndex].StatusValue = true;
        CheckMatchStartPossibility();
    }

    private void CheckMatchStartPossibility()
    {
        bool canProceed = true;
        foreach(TeamConnectionPanel panel in teamConnectionPanels)
        {
            if (!panel.StatusValue)
            {
                canProceed = false;
                break;
            }
        }
        ProceedToMatchButton.interactable = canProceed;
    }

    public void ShowFireQuestionConfirmation()
    {
        if (questionInProgress || currentSelectedTeamIndex == -1)
            return;
        FireQuestionConfirmationPanel.SetActive(true);
    }

    public void FireQuestion()
    {
        lastSelectedTeamIndex = currentSelectedTeamIndex;
        questionInProgress = true;
        Question[] questions = NetworkBehavior.CurrentMatch.teams[currentSelectedTeamIndex].questions;
        Question question = questions[Random.Range(0, questions.Length)];
        NetworkBehavior.FireQuestion(currentSelectedTeamIndex, NetworkBehavior.Teams[currentSelectedTeamIndex], question, false);
        questionFired = question;
        
        controlButtons[lastSelectedTeamIndex].SetQuestions(NetworkBehavior.Teams[lastSelectedTeamIndex].questionsNumber + 1);

        OnQuestionFired();
    }

    public void ShowRefireQuestionConfirmation()
    {
        if (!questionInProgress || currentSelectedTeamIndex == -1)
            return;
        RefireQuestionConfirmationPanel.SetActive(true);
    }

    public void RefireQuestion()
    {
        if (currentSelectedTeamIndex == -1 || !questionInProgress)
            return;
        lastSelectedTeamIndex = currentSelectedTeamIndex;
        NetworkBehavior.RefireQuestion(currentSelectedTeamIndex, NetworkBehavior.Teams[currentSelectedTeamIndex], questionFired);

        OnQuestionFired();
    }

    private void OnQuestionFired()
    {
        RevealOptionsPanel.SetActive(true);
        FireQuestionButton.interactable = false;
        ResetTimerButton.interactable = true;
        RefireQuestionButton.interactable = true;
    }

    public void ShowResetTimerConfirmation()
    {
        ResetTimerConfirmationPanel.SetActive(true);
    }

    public void ResetTimer()
    {
        NetworkBehavior.ResetTimer(lastSelectedTeamIndex);
    }

    public void FireShowOptions()
    {
        NetworkBehavior.FireShowOptions(currentSelectedTeamIndex);
    }

    public void LockAnswer(int index)
    {
        lockedAnswerIndex = index;
        RevealAnswerPanel.SetActive(true);
    }

    public void FireRevealContestantAnswer()
    {
        NetworkBehavior.FireRevealContestantAnswer(currentSelectedTeamIndex, lockedAnswerIndex);
        VerifyAnswer(lockedAnswerIndex);

        FireQuestionButton.interactable = true;
        ResetTimerButton.interactable = false;
    }

    public void VerifyAnswer(int index)
    {
        if (questionFired.options[index].is_correct == 1)
        {
            NetworkBehavior.SetPoints(lastSelectedTeamIndex, NetworkBehavior.Teams[lastSelectedTeamIndex].points + 10);
            controlButtons[lastSelectedTeamIndex].SetPoints(NetworkBehavior.Teams[lastSelectedTeamIndex].points + 10);
        }
        questionInProgress = false;
        DeselectCurrentTeam();
    }

    public void TimeElapsed()
    {
        questionInProgress = false;
        DeselectCurrentTeam();
    }

    private void DeselectCurrentTeam()
    {
        TeamControlButton.CurrentSelected.Select(false);
        currentSelectedTeamIndex = -1;
        lastSelectedTeamIndex = -1;
    }

    public void ToggleLeaderboard()
    {
        if (!leaderboardShown)
            NetworkBehavior.FireShowLeaderboard();
        else
            NetworkBehavior.FireHideLeaderboard();
        leaderboardShown = !leaderboardShown;
    }

    public void AskQuestionToAudience()
    {
        Question[] questions = NetworkBehavior.CurrentMatch.teams[0].questions;
        Question question = questions[Random.Range(0, questions.Length)];
        NetworkBehavior.AskQuestionToAudience(question);
        RevealAudienceAnswerPanel.SetActive(true);
    }

    public void RevealAudienceAnswer()
    {
        NetworkBehavior.RevealAudienceAnswer();
    }

    public void ShowExpertLifelineRequest()
    {
        LifelineConfirmation.SetActive(true);
        LifelineConfirmationImage.sprite = ExpertSprite;
        LifelineText.text = NetworkBehavior.Teams[lastSelectedTeamIndex].name.ToUpper() + " IS REQUESTING AN EXPERT QUESTION LIFELINE";
        AcceptLifelineRequestButton.onClick.RemoveAllListeners();
        AcceptLifelineRequestButton.onClick.AddListener(() =>
        {
            NetworkBehavior.AcceptExpertLifeline(lastSelectedTeamIndex);
        });
    }

    public void ShowAudienceLifelineRequest()
    {
        LifelineConfirmation.SetActive(true);
        LifelineConfirmationImage.sprite = AudienceSprite;
        LifelineText.text = NetworkBehavior.Teams[lastSelectedTeamIndex].name.ToUpper() + " IS REQUESTING AN AUDIENCE QUESTION LIFELINE";
        AcceptLifelineRequestButton.onClick.RemoveAllListeners();
        AcceptLifelineRequestButton.onClick.AddListener(() =>
        {
            NetworkBehavior.AcceptAudienceLifeline(lastSelectedTeamIndex, questionFired);
        });
    }

    public void ShowSwapLifelineRequest()
    {
        LifelineConfirmation.SetActive(true);
        LifelineConfirmationImage.sprite = SwapSprite;
        LifelineText.text = NetworkBehavior.Teams[lastSelectedTeamIndex].name.ToUpper() + " IS REQUESTING A SWAP LIFELINE";
        AcceptLifelineRequestButton.onClick.RemoveAllListeners();
        AcceptLifelineRequestButton.onClick.AddListener(() =>
        {
            Question[] questions = NetworkBehavior.CurrentMatch.teams[currentSelectedTeamIndex].questions;
            Question question = questions[Random.Range(0, questions.Length)];
            NetworkBehavior.FireQuestion(currentSelectedTeamIndex, NetworkBehavior.Teams[currentSelectedTeamIndex], question, true);
            questionFired = question;

            OnQuestionFired();
        });
    }

    public void ShowHalfLifelineRequest()
    {
        LifelineConfirmation.SetActive(true);
        LifelineConfirmationImage.sprite = HalfSprite;
        LifelineText.text = NetworkBehavior.Teams[lastSelectedTeamIndex].name.ToUpper() + " IS REQUESTING A 50/50 LIFELINE";
        AcceptLifelineRequestButton.onClick.RemoveAllListeners();
        List<int> indexesToRemove = new List<int>();
        for(int i = 0; i < 2; i++)
        {
            int index = -1;
            do
            {
                index = Random.Range(0, questionFired.options.Length);
            }
            while (questionFired.options[index].is_correct == 1 || indexesToRemove.Contains(index));
            indexesToRemove.Add(index);
        }
        AcceptLifelineRequestButton.onClick.AddListener(() =>
        {
            NetworkBehavior.AcceptHalfLifeline(lastSelectedTeamIndex, indexesToRemove);
        });
    }

    public void ShowRevealPollResult()
    {
        RevealPollResultPanel.SetActive(true);
    }

    public void ConfirmRevealPollResult()
    {
        NetworkBehavior.ConfirmRevealPollResult(lastSelectedTeamIndex);
    }

    public void SetActionIndex(int index)
    {
        controlActionIndex = index;
    }

    public void InvokeAction()
    {
        if (controlActionIndex == -1)
            return;
        switch (controlActionIndex)
        {
            case 0:
                ShowFireQuestionConfirmation();
                break;
            case 1:
                ShowResetTimerConfirmation();
                break;
            case 2:
                ShowRefireQuestionConfirmation();
                break;
            case 3:
                ShowAskAudienceConfirmation();
                break;
            case 4:
                WinnerSelectionPanel.SetActive(true);
                AnnouncementPanel.SetActive(false);
                break;
        }
        ResetActionIndex();
    }

    public void ResetActionIndex()
    {
        controlActionIndex = -1;
    }

    public void ShowDisconnectionNotification(int teamIndex)
    {
        DisconnectionWindow.SetActive(true);
        DisconnectionText.text = $"{NetworkBehavior.Teams[teamIndex].name} HAS BEEN DISCONNECTED".ToUpper();
        //print($"{NetworkBehavior.Teams[teamIndex].name} HAS BEEN DISCONNECTED".ToUpper());
    }
}