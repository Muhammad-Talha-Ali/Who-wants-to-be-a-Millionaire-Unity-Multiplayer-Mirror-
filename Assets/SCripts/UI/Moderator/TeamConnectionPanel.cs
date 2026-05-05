using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamConnectionPanel : MonoBehaviour
{
    public Image Status;
    public Image Logo;
    public bool StatusValue;
    public TMP_Text Name;
    public TMP_Text Code;

    [SerializeField]
    private Sprite[] sprites;

    public void SetConnected(bool connected)
    {
        Status.sprite = connected ? sprites[0] : sprites[1];
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
