using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAT : MonoBehaviour
{ [SerializeField]
    public GameObject Center;

    void Start()
    {
        transform.LookAt(Center.transform);
    }
}
