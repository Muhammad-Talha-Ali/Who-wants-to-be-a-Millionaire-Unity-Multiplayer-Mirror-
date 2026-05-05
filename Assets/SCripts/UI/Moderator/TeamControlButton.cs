using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TeamControlButton : MonoBehaviour
{
    public static TeamControlButton CurrentSelected;

    public TMP_Text Name;
    public TMP_Text Points;
    public TMP_Text Questions;
    public TMP_Text SelectedName;
    public TMP_Text SelectedPoints;
    public TMP_Text SelectedQuestions;
    public Image Logo;

    public GameObject SelectedPanel;

    public Button button { get; private set; }
    private Image image;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    public void SetSelected()
    {
        if (CurrentSelected)
            CurrentSelected.Select(false);
        Select(true);
        CurrentSelected = this;
    }

    public void Select(bool value)
    {
        SelectedPanel.SetActive(value);
    }

    public void SetPoints(int points)
    {
        Points.text = "POINTS: " + points;
        SelectedPoints.text = "POINTS: " + points;
    }

    public void SetQuestions(int questionsNumber)
    {
        Questions.text = questionsNumber + "\n<size=5> OUT OF </size>\n10";
        SelectedQuestions.text = questionsNumber + "\n<size=5> OUT OF </size>\n10";
    }
}
