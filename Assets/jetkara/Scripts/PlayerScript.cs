using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
	bool dead;
	public AudioClip[] auClip;
	public GameObject fire;

	void Start()
	{
		dead = false;
		GetComponent<AudioSource>().clip = auClip[0];
	}
	void FixedUpdate()
	{
		Vector3 fwd = transform.TransformDirection(Vector3.forward);
		RaycastHit hit;
		if (Physics.Raycast(transform.position,fwd, out hit, 20))
		{

			if (hit.collider.gameObject.tag == "Lower")
			{
				print("There is something in front of the object!");
				/*Jump();*/
			}
		}
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !dead)
		{
			RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			
			if(hit.collider == null)
			{
				Jump();
			}
		}
	}

	void Jump()
	{
		fire.SetActive (true);
		GetComponent<AudioSource>().Play();
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		GetComponent<Rigidbody2D>().AddForce(Vector2.up * 200);
	}

	void OnTriggerEnter2D(Collider2D col) 
	{
		if (!dead)
		{
			if (col.tag == "Score")
			{
				GameObject.FindObjectOfType<GameManager>().Score++;
				Destroy(col.gameObject);
			}
			else if (col.tag == "Finish")
			{
				dead = true;
				GetComponent<AudioSource>().clip = auClip[1];
				GetComponent<AudioSource>().Play();
				Invoke("BackToMain", 1.5f);
			}
		}
	}

	void BackToMain()
	{
       // SceneManager.LoadScene("MainMenu");
        SceneManager.LoadScene("MainMenu");
	}
}
