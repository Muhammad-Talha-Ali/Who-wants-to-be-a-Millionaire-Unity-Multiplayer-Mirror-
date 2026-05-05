using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int rotationSpeedX;
    public int rotationSpeedY;
    public int rotationSpeedZ;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate (new Vector3(Time.deltaTime * rotationSpeedX, Time.deltaTime*rotationSpeedY, Time.deltaTime * rotationSpeedZ),Space.Self);
    }
}
