using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TeamSelectionButton : MonoBehaviour
{
    public Team Team;

    public TMP_Text Initials;
    public TMP_Text Name;
    public TMP_Text SelectedInitials;
    public TMP_Text SelectedName;
    public Image Logo;
    private bool selected = false;
    private bool logoLoaded = false;

    [SerializeField]
    private GameObject selectedPanel;

    public Button button { get; private set; }
    private Image image;

    private void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if (Team.logoReady && !logoLoaded)
        {
            //Logo.sprite = Team.logoSprite;
            logoLoaded = false;
        }
    }

    public void ToggleSelected()
    {
        selected = !selected;
        selectedPanel.SetActive(selected);
    }

    public IEnumerator LoadLogo(Team team)
    {
        while (!team.logoReady)
        {
            yield return null;
        }
        Logo.sprite = team.logoSprite;
    }
}
