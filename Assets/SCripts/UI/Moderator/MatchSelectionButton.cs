using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchSelectionButton : MonoBehaviour
{
    public static MatchSelectionButton CurrentSelected;

    private Image image;

    public TMP_Text Date;

    public Transform TeamImagesContainer;

    public Button button { get; private set; }

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

    private void Select(bool value)
    {
        if (value)
        {
            image.color = Color.green;
        }
        else
        {
            image.color = Color.white;
        }
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
