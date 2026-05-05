using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;

public class AudienceInterface : BaseInterface
{
    #region PARAMETERS

    [Header("Screens")]
    [SerializeField] private GameObject connectionScreen;
    [SerializeField] private GameObject pollScreen;
    [SerializeField] private GameObject questionScreen;
    [SerializeField] private GameObject congratulationsScreen;
    [SerializeField] private GameObject keepInTouchScreen;

    [Header("Connection Inputs")]
    [SerializeField] private TMP_InputField serverAddress;
    [SerializeField] private TMP_InputField code;
    [SerializeField] private TMP_InputField name;

    [Header("Forms")]
    [SerializeField] private QuestionForm questionForm;
    [SerializeField] private QuestionForm optionsForm;

    #endregion

    #region CACHES

    private AudienceNetworkManager networkManager;
    [HideInInspector]
    public AudienceBehavior NetworkBehavior;

    #endregion

    #region STATES

    private int answerIndex = -1;
    private bool isLifeline = false;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        networkManager = FindObjectOfType<AudienceNetworkManager>();
    }

    public void Connect()
    {
        networkManager.networkAddress = serverAddress.text;
        networkManager.Code = code.text;
        networkManager.Name = name.text;
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
        connectionScreen.SetActive(false);
        keepInTouchScreen.SetActive(true);
    }

    public void ChooseAnswer(int index)
    {
        answerIndex = index;
    }

    #region Audience Poll Lifeline

    public void ShowPollOptions(Question question)
    {
        isLifeline = true;
        pollScreen.SetActive(true);
        keepInTouchScreen.SetActive(false);
        optionsForm.InitializeTimer();
        optionsForm.ShowOptions(question.options);
    }

    #endregion

    #region Audience Question

    public void ShowQuestion(Question question)
    {
        isLifeline = false;
        questionScreen.SetActive(true);
        keepInTouchScreen.SetActive(false);
        questionForm.SetQuestion(question.question, null);
        questionForm.ShowOptions(question.options);
    }

    public void HideQuestionScreen()
    {
        questionScreen.SetActive(false);
        keepInTouchScreen.SetActive(true);
    }

    public void LockAnswer()
    {
        NetworkBehavior.LockAnswer(answerIndex, isLifeline);
        questionForm.SetButtonLocked(answerIndex);
        questionForm.StopTimer();
        questionScreen.SetActive(false);
        keepInTouchScreen.SetActive(true);
    }

    public void ShowCongratulationsMessage()
    {
        keepInTouchScreen.SetActive(false);
        congratulationsScreen.SetActive(true);
    }

    #endregion
}
