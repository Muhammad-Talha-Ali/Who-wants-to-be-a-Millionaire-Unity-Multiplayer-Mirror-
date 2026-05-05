using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    public GameObject[] options;
    public List<QuestionnAnswer> QnA;
    public int currentQuestion;
    public int score;

    public GameObject quizPanel;
    public GameObject gameOverPanel;

    public int totalQuestions = 0;
    public Text QuestionTxt;
    public Text ScoreTxt;
    public Text GameOverScoreText;



    void Start()
    {
        totalQuestions = QnA.Count;
        gameOverPanel.SetActive(false);

        generateQuestion();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        quizPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        ScoreTxt.text = score + "/" + totalQuestions;
    }

    private void generateQuestion()
    {
        if (QnA.Count > 0)
        {
            currentQuestion = Random.Range(0, QnA.Count);
            QuestionTxt.text = QnA[currentQuestion].Questions;
            SetAnswers();
        }
        else
        {
            Debug.Log("Out of qustion");
            GameOver();
        }
    }
    public void Wrong()
    {
        QnA.RemoveAt(currentQuestion);
        generateQuestion();
    }
    public void correct()
    {
        QnA.RemoveAt(currentQuestion);
        score++;
        generateQuestion();
    }
    private void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswersScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<Text>().text = QnA[currentQuestion].Answers[i];
            if(QnA[currentQuestion].CorrectAnswer == i + 1)
            {
                options[i].GetComponent<AnswersScript>().isCorrect = true;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        ScoreTxt.text = score + "/" + totalQuestions;
        GameOverScoreText.text = score + "/" + totalQuestions;
    }
}
