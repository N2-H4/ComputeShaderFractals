using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    Transform t;
    void Start()
    {
        t = GetComponent<Transform>();
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            t.position += t.forward * Time.deltaTime * 0.4f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            t.position -= t.forward * Time.deltaTime * 0.4f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            t.position += t.right * Time.deltaTime * 0.4f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            t.position -= t.right * Time.deltaTime * 0.4f;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            t.position += t.up * Time.deltaTime * 0.4f;
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            t.position -= t.up * Time.deltaTime * 0.4f;
        }
    }
}
