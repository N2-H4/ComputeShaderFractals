using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour
{
    Transform t;

    public float turnSpeed = 0.0f;
    float movementSpeed = 0.5f;

    public float minTurnAngle = -90.0f;
    public float maxTurnAngle = 90.0f;
    private float rotX;
    void Start()
    {
        t = GetComponent<Transform>();
        
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            t.position += t.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            t.position -= t.forward * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            t.position += t.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            t.position -= t.right * Time.deltaTime * movementSpeed;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            t.position += t.up * Time.deltaTime * movementSpeed;
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            t.position -= t.up * Time.deltaTime * movementSpeed;
        }
        if(Input.GetMouseButtonDown(2))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            turnSpeed = 3.0f;
        }
        if(Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            turnSpeed = 0.0f;
        }

        MouseAiming();
    }

    void MouseAiming()
    {
        // get the mouse inputs
        float y = Input.GetAxis("Mouse X") * turnSpeed;
        rotX += Input.GetAxis("Mouse Y") * turnSpeed;

        // clamp the vertical rotation
        rotX = Mathf.Clamp(rotX, minTurnAngle, maxTurnAngle);

        // rotate the camera
        t.eulerAngles = new Vector3(-rotX, transform.eulerAngles.y + y, 0);
    }

    public void setMovementSpeed(float val)
    {
        movementSpeed = val;
    }
}
