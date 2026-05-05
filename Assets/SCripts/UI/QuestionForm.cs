using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class QuestionForm : MonoBehaviour
{
    ContestantInterface interf;
    ServerInterface serverInterf;
    AudienceInterface audienceInterf;

    public GameObject QuestionPanel;
    public TMP_Text QuestionText;

    public GameObject OptionsPanel;
    public Button[] Options;
    public TMP_Text Timer;

    public Color NormalColor;
    public Color CorrectColor;
    public Color WrongColor;
    public Color LockedColor;

    [SerializeField] private float timerValue = 180;
    private float timer;
    private float lifetimeTimer;
    private bool timerOn = false;
    private bool lifetimeTimerOn = false;

    public Button ExpertLifelineButton;
    public Button AudienceLifelineButton;
    public Button SwapLifelineButton;
    public Button HalfLifelineButton;

    public GameObject[] PollResults;

    private void Awake()
    {
        interf = FindObjectOfType<ContestantInterface>();
        serverInterf = FindObjectOfType<ServerInterface>();
        audienceInterf = FindObjectOfType<AudienceInterface>();
    }

    public void SetQuestion(string question, Team team)
    {
        QuestionPanel.SetActive(true);
        QuestionText.text = question;

        if (team != null && ExpertLifelineButton)
        {
            ExpertLifelineButton.interactable = team.expertLifeline;
            AudienceLifelineButton.interactable = team.audienceLifeline;
            SwapLifelineButton.interactable = team.swapLifeline;
            HalfLifelineButton.interactable = team.halfLifeline;
        }

        InitializeTimer();
        InitializeOptions();
    }

    public void InitializeTimer()
    {
        timerOn = false;
        timer = timerValue;
        Timer.text = ((int)timer).ToString();
    }

    private void InitializeOptions()
    {
        OptionsPanel.SetActive(false);
        foreach (Button option in Options)
        {
            option.interactable = true;
        }
    }

    public void ShowOptions(Option[] options)
    {
        OptionsPanel.SetActive(true);
        int i = 0;
        foreach(Button option in Options)
        {
            option.GetComponentInChildren<TMP_Text>().text = $"{options[i].key}: {options[i].value}";
            option.GetComponent<Image>().color = Color.white;
            i++;
        }
        StartTimer();
    }

    public void StartTimer()
    {
        timerOn = true;
    }

    public void StartLifetimeTimer(float duration)
    {
        lifetimeTimer = duration;
        timerOn = false;
        lifetimeTimerOn = true;
    }

    public void StopTimer()
    {
        timerOn = false;
    }

    public void StopLifetimeTimer()
    {
        timerOn = true;
        lifetimeTimerOn = false;
    }

    public float GetTimer()
    {
        return timer;
    }

    public void SetButtonLocked(int optionIndex)
    {
        Options[optionIndex].GetComponent<Image>().color = LockedColor;
    }

    public void SetButtonCorrect(int optionIndex)
    {
        Options[optionIndex].GetComponent<Image>().color = CorrectColor;
        StartCoroutine(HideQuestion(3));
    }

    public void SetButtonWrong(int optionIndex)
    {
        Options[optionIndex].GetComponent<Image>().color = WrongColor;
        StartCoroutine(HideQuestion(3));
    }

    public void DisableOptions(List<int> indexesToDisable)
    {
        foreach(int index in indexesToDisable)
        {
            Options[index].interactable = false;
        }
    }

    private void Update()
    {
        if (timerOn)
        {
            timer -= Time.deltaTime;
            Timer.text = ((int)timer).ToString();
            if(timer <= 0)
            {
                timerOn = false;
                interf?.TimeElapsed();
                StartCoroutine(HideQuestion(0));
            }
        }
        else if (lifetimeTimerOn)
        {
            lifetimeTimer -= Time.deltaTime;
            //Timer.text = ((int)timer).ToString();
            if (lifetimeTimer <= 0)
            {
                StopLifetimeTimer();
            }
        }
    }

    public void ResetTimer()
    {
        timer = 180;
    }

    public void SetTimer(float time)
    {
        timer = time;
    }

    IEnumerator HideQuestion(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        OptionsPanel.SetActive(false);
        QuestionPanel.SetActive(false);
        interf?.HideQuestionScreen();
        serverInterf?.HideQuestionScreen();
        audienceInterf?.HideQuestionScreen();
    }

    public void SetPollResult(int index, int value)
    {
        PollResults[index].SetActive(true);
        PollResults[index].GetComponentInChildren<TMP_Text>().text = value.ToString();
    }

    public void DisableExpertLifeline()
    {
        ExpertLifelineButton.interactable = false;
    }

    public void DisableAudienceLifeline()
    {
        AudienceLifelineButton.interactable = false;
    }

    public void DisableHalfLifeline()
    {
        HalfLifelineButton.interactable = false;
    }
}
