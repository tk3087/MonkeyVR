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


    public struct Location
    {
        public Vector3 locPos;
        public float locHeading;
    }

    Location[] preSetLocations = new Location[4];

    
    

    Rigidbody rb;

    public float moveX;
    public float moveY;
    float effective_X;
    float effective_Y;
    public float press;
    public bool randomHeadingOnReset = false;
    [ShowOnly]
    public float currentSpeed;
    [ShowOnly]
    public float currentRot;
    public float TravelTime = 4; // Time in seconds to travel 2R of polygon
    public float RotSpeed = 360.0f;
    private float prevX;
    private float prevY;
    private int ArenaJoysticMode;

    public static PlayerMovement sharedmovement;

    public SteamVR_Behaviour_Pose controllerPose;


    // Start is called before the first frame update

    void Awake()
    {
        sharedmovement = this;

        if (PlayerPrefs.GetInt("ArenaRandomHeadingEnabled", 0) == 0)
            randomHeadingOnReset = false;
        else
            randomHeadingOnReset = true;
        
        // Position infront of TV2
        preSetLocations[2].locPos = new Vector3(2.06f, 0.6f, 1.272f);
        preSetLocations[2].locHeading = 190.0f;

        // Position infront of TV3
        preSetLocations[3].locPos = new Vector3(-2.06f, 0.6f, 1.272f);
        preSetLocations[3].locHeading = -190.0f;

        // Position max distance infron of TV1
        preSetLocations[1].locPos = new Vector3(0.145f, 0.6f, 2.42190f);
        preSetLocations[1].locHeading = -179.0f;

        //Position at the center of the Arena

        preSetLocations[0].locPos = new Vector3(0.0f, 0.6f, 0.0f);
        preSetLocations[0].locHeading = 180.0f;




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

        float initialPlayerRot = 0;
        Vector3 initialPosition = new Vector3(0.0f, 0.2f, 0.0f);

        switch (ArenaGame.theArenaGame.ArenaTrainigMode)
        {
            case ArenaGame.TrainingMode.None:

                //Set player at random direction initially.
                initialPlayerRot = 360.0f*UnityEngine.Random.value;
                break;

            case ArenaGame.TrainingMode.ScreenFront:
                //Set player infront of Screen 1.
                initialPlayerRot = 60.0f;
                initialPosition.x = 1.47f;
                initialPosition.y = 0.2f;
                initialPosition.z = 0.664f;
                break;

            case ArenaGame.TrainingMode.ScreenStraight:
                //Set player infront of Screen 1 at the middle of the Arena.
                initialPlayerRot = 60.0f;
                break;



        }

        Quaternion init_Y_Rotation = Quaternion.Euler(0f, initialPlayerRot, 0f);
        rb.MoveRotation(init_Y_Rotation);
        rb.MovePosition(initialPosition);

        ArenaJoysticMode = ArenaGame.theArenaGame.ArenaJoysticMode;
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



        switch (ArenaGame.theArenaGame.ArenaTrainigMode)
        {
            case ArenaGame.TrainingMode.ScreenFront:
                return;

            case ArenaGame.TrainingMode.ScreenStraight:

                if (ArenaGame.theArenaGame.resetPostion == true)
                {
                    float initialPlayerRot = 60;
                    Vector3 initialPosition = new Vector3(0.0f, 0.2f, 0.0f);
                    Quaternion init_Y_Rotation = Quaternion.Euler(0f, initialPlayerRot, 0f);
                    rb.MoveRotation(init_Y_Rotation);
                    rb.MovePosition(initialPosition);
                    ArenaGame.theArenaGame.resetPostion = false;
                    return;
                }

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


                if (ArenaJoysticMode == 0)
                    moveX = 0;
                else
                    moveY = 0;
                break;

            case ArenaGame.TrainingMode.None:

                if (ArenaGame.theArenaGame.resetPostion == true)
                {
                    
                   

                    float initialPlayerRot = preSetLocations[ArenaGame.theArenaGame.presetPositionNumber].locHeading;
                    //Vector3 initialPosition = new Vector3(0.0f, 0.2f, 0.0f);
                    
                    
                    Vector3 initialPosition = preSetLocations[ArenaGame.theArenaGame.presetPositionNumber].locPos;

                    
                    
                    //Overide heading if random heading is enabled.

                    if (randomHeadingOnReset==true && ArenaGame.theArenaGame.presetPositionNumber==0)
                        initialPlayerRot = 360.0f * UnityEngine.Random.value;

                    Quaternion init_Y_Rotation = Quaternion.Euler(0f, initialPlayerRot, 0f);
                    rb.MoveRotation(init_Y_Rotation);
                    rb.MovePosition(initialPosition);
                    ArenaGame.theArenaGame.resetPostion = false;
                    return;
                }

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
                break;


        }


        if (ArenaJoysticMode == 0)
        {
            effective_X = moveX;
            effective_Y = moveY;

        }
        else if (ArenaJoysticMode == 1)
        {
            effective_X = -moveY;
            effective_Y = moveX;
        }


        //moveDirection = orientation.forward * moveY + orientation.right * moveX;

        moveDirection = orientation.forward * effective_Y + orientation.right * effective_X;


        rb.AddForce(moveDirection.normalized * movementMultiplier * 0.2f, ForceMode.Force);
        if (moveX < 0.5f && moveX > -0.5f)
        {
            moveX = 0f;
        }
        if (moveY < 0.99f && moveY > -0.99f)
        {
            moveY = 0f;
        }

        if (ArenaJoysticMode == 0)
        {
            effective_X = moveX;
            effective_Y = moveY;

        }
        else if (ArenaJoysticMode == 1)
        {
            effective_X = -moveY;
            effective_Y = moveX;
        }
        //currentSpeed = moveY * movementMultiplier;
        //currentRot = moveX * RotSpeed;

        currentSpeed = effective_Y * movementMultiplier;
        currentRot = effective_X * RotSpeed;

        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);
        Quaternion deltaRotation = Quaternion.Euler(0f, currentRot * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
