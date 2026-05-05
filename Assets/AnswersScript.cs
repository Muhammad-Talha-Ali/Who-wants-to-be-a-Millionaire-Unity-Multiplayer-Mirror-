using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnswersScript : MonoBehaviour
{
    // Start is called before the first frame update

    public bool isCorrect = false;
    public QuizManager quizManager;
    public void Answer()
    {
        if (isCorrect)
        {
            Debug.Log("correct ans");
            quizManager.correct();

        }
        else
        {
            Debug.Log("wrong ans");
            quizManager.Wrong();

        }
    }

  
}
