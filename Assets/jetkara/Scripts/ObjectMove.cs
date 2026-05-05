using UnityEngine;


public class ObjectMove : MonoBehaviour 
{
	void FixedUpdate () 
	{
		transform.position = new Vector3(transform.position.x - 0.3f, transform.position.y , 27f);

		if (transform.position.x <= -37.5f)
		{
			Destroy(gameObject);
		}
	}
}
