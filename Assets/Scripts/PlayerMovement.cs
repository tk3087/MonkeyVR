using System.Collections;
using System.Collections.Generic;
using System;
using System.IO.Ports;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using Valve.VR;
using UnityEngine.VR;




public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    Transform orientation = null;

    [Header("Movement")]
    [SerializeField]
    float movementMultiplier;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    
    
    


    Rigidbody rb;
    
    public float moveX;
    public float moveY;
    public float press;
    [ShowOnly]
    public float currentSpeed;
    [ShowOnly]
    public float currentRot;
    public float TravelTime = 4; // Time in seconds to travel 2R of polygon
    public float RotSpeed = 360.0f;
    private float prevX;
    private float prevY;

    public static PlayerMovement sharedmovement;

    public SteamVR_Behaviour_Pose controllerPose;

    // Start is called before the first frame update

    void Awake()
    {
        sharedmovement = this;
      
       

    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //tomkil movementMultiplier = 12.0f / TravelTime;
        movementMultiplier = 6f / TravelTime;
        //movementMultiplier = 1.5f;
        // 5.25 second to travel 2R of polygon
        // Travel time = 4 (sec desired travel time for 2R);
        PlayerPrefs.SetFloat("MovementMultiplier", movementMultiplier);

        //Set player at random direction initially.

        Quaternion rand_Y_Rotation = Quaternion.Euler(0f, 360 * UnityEngine.Random.value, 0f);
        rb.MoveRotation(rand_Y_Rotation);
        

    }

    // Update is called once per frame
    void Update()
    {
        CTIJoystick joystick = CTIJoystick.current;
        press = joystick.leftButton.ReadValue();
    }

    private void FixedUpdate()
    {
        MyInput();
    }

    void MyInput()
    {
        CTIJoystick joystick = CTIJoystick.current;
        moveX = joystick.x.ReadValue();
        moveY = joystick.y.ReadValue();

        if (moveX < 0.0f)
        {
            moveX += 1.0f;
        }
        else if (moveX > 0.0f)
        {
            moveX -= 1.0f;
        }
        else if (moveX == 0)
        {
            if (prevX < 0.0f)
            {
                moveX -= 1.0f;
            }
            else if (prevX > 0.0f)
            {
                moveX += 1.0f;
            }
        }
        prevX = moveX;

        if (moveY < 0.0f)
        {
            moveY += 1.0f;
        }
        else if (moveY > 0.0f)
        {
            moveY -= 1.0f;
        }
        else if (moveY == 0)
        {
            if (prevY < 0.0f)
            {
                moveY -= 1.0f;
            }
            else if (prevY > 0.0f)
            {
                moveY += 1.0f;
            }
        }
        prevY = moveY;


        moveDirection = orientation.forward * moveY + orientation.right * moveX;
        rb.AddForce(moveDirection.normalized * movementMultiplier * 0.2f, ForceMode.Force);
        if (moveX < 0.5f && moveX > -0.5f)
        {
            moveX = 0f;
        }
        if (moveY < 0.99f && moveY > -0.99f)
        {
            moveY = 0f;
        }
        currentSpeed = moveY * movementMultiplier;
        currentRot = moveX * RotSpeed;
        
        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);
        Quaternion deltaRotation = Quaternion.Euler(0f, currentRot * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
