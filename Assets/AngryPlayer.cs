using UnityEngine;
using UnityEngine.SceneManagement;

public class AngryPlayer : MonoBehaviour
{
	bool dead;
	public AudioClip[] auClip;
	public static bool fall;
	void Start()
	{
		dead = false;
		GetComponent<AudioSource>().clip = auClip[0];
	}
	/*void FixedUpdate()
	{
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		Vector3 bot = transform.TransformDirection(Vector3.down);
		RaycastHit hit;
		if (Physics.Raycast(transform.position, fwd, out hit, 20))
		{

			if (hit.collider.gameObject.tag == "LowerPipe" && !fall)
			{
				print("There is something in front of the object!");
				Jump();
			}
		}
		if (Physics.Raycast(transform.position, bot, out hit, 5))
		{

			if (hit.collider.gameObject.tag == "Finish" && !fall)
			{
				print("There is something in front of the object!");
				Jump();
			}
		}
		if (transform.position.y < -2.5f && !fall)
        {
			StrongJump();
        }
	}*/

    void Update()
    {		if(transform.position.y < -10 && !dead)
        {
			dead = true;
			GetComponent<AudioSource>().clip = auClip[1];
			GetComponent<AudioSource>().Play();
			Invoke("BackToMain", 1.5f);
		}
        if (Input.GetMouseButtonDown(0) && !dead)
        {
            fall = true;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == null)
            {
                Jump();
            }
        }
    }

    void Jump()
	{
		GetComponent<AudioSource>().Play();
		GetComponent<Rigidbody>().velocity = Vector2.zero;
		GetComponent<Rigidbody>().AddForce(Vector2.up * 200);
	}
	void StrongJump()
	{
		GetComponent<AudioSource>().Play();
		GetComponent<Rigidbody>().velocity = Vector2.zero;
		GetComponent<Rigidbody>().AddForce(Vector2.up * 600);
	}

	void OnTriggerEnter(Collider col)
	{
		if (!dead)
		{
			if (col.tag == "Score")
			{
				print("Score up");
				GameObject.FindObjectOfType<GameManager>().Score++;
				Destroy(col.gameObject);
			}
			if (fall) {
			 if (col.tag == "Finish" ||col.tag == "LowerPipe" || col.tag == "UperPipe")
				{
					dead = true;
					GetComponent<AudioSource>().clip = auClip[1];
					GetComponent<AudioSource>().Play();
					Invoke("BackToMain", 1.5f);
				}
			} }
	}

	void BackToMain()
	{
		// SceneManager.LoadScene("MainMenu");
		fall = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
