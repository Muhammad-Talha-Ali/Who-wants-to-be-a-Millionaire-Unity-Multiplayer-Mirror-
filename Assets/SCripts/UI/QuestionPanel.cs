using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionPanel : MonoBehaviour
{
    public Animator animator;
    public Scrollbar teamsScrollbar;
    public float scrollSpeed;
    public int direction = 1;
    public float waitTime = 2;
    private float currentWaitTime;

    private void Start()
    {
        currentWaitTime = waitTime;
    }

    public void ShowOptions()
    {
        animator.Play("QuestionShrink");
    }

    private void Update()
    {
        if(currentWaitTime > 0)
        {
            currentWaitTime -= Time.deltaTime;
            return;
        }
        teamsScrollbar.value += direction * scrollSpeed / 10 * Time.deltaTime;
        if(teamsScrollbar.value <= 0 || teamsScrollbar.value >= 1)
        {
            print("Change");
            currentWaitTime = waitTime;
            direction = -direction;
            if(teamsScrollbar.value <= 0)
            {
                teamsScrollbar.value = 0.05f;
            }
            else if (teamsScrollbar.value >= 1)
            {
                teamsScrollbar.value = 9.95f;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowOptions();
        }
    }


}
