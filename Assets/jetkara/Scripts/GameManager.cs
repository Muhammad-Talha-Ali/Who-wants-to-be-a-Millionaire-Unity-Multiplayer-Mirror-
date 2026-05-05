using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public GameObject objects;

	public Text scoreLabel;
	public Text TimerLabel;
	public static int score;
	public float waitTime;
	public float ResetTime;
	public GameObject Instructions;
	public int Score
	{
		set
		{
			score = value;

			scoreLabel.text ="Score : "+ Score.ToString();
		}
		get
		{
			return score;
		}
	}
    private void Update()
    {
		if (Input.GetMouseButtonDown(0))
			{
			Time.timeScale = 1;
        }

		if (waitTime > -1)
        {
            if (waitTime > 0.5f)
            {
                Instructions.SetActive(false);
            }
            TimerLabel.text = "Timer : " + waitTime.ToString(); 
			waitTime += Time.deltaTime;
        }
        else
        {	
		TimerLabel.text = "Timer : 0";
			/*AngryPlayer.fall = true;*/
        }

	}
    void Start () 
	{
		Time.timeScale = 0;
		score = 0;
		waitTime=ResetTime;

		InvokeRepeating("CreateObjects", 0,3);
	}

	void CreateObjects()
	{
		Instantiate(objects, new Vector3(100, Random.Range(-2f, 2.1f) , 27f) , Quaternion.identity);
	}
}
