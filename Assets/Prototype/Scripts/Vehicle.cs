using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    Rigidbody RB;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RB.velocity = new Vector3(0.0f, 0.0f, 4.0f);
    }
}
