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
using static SerialArena;
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

    Location[] preSetLocations = new Location[12];

    
    

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
    public float TravelTime = 7.5f; // Time in seconds to travel 2R of polygon
    public float RotSpeed = 120.0f; //360.0f;
    private float prevX;
    private float prevY;
    private int ArenaJoysticMode;
    
    private bool inBackwardMode = false;
    private bool inRestInterval = false;
    
    private float startTimeMovingBack;
    private float durationBackMoveToGetReward;

    private float timeNow;

    private float startTimeRestInterval;
    private float durationRestInterval;


    public static PlayerMovement sharedmovement;

    public SteamVR_Behaviour_Pose controllerPose;


    const float minRotSpeed = 30.0f;
    const float maxRotSpeed = 120.0f;
    const float stepRotSpeed = 10.0f;

    const float minTimeTravel = 0.5f;
    const float maxTimeTravel = 12.0f;
    const float stepLinSpeed = 0.5f;

    const float distanceToTravel = 6.0f;




    // Start is called before the first frame update

    void Awake()
    {
        sharedmovement = this;

        if (PlayerPrefs.GetInt("ArenaRandomHeadingEnabled", 0) == 0)
            randomHeadingOnReset = false;
        else
            randomHeadingOnReset = true;
        
        // Position Q 
        preSetLocations[2].locPos = new Vector3(1.77f, 0.6f, 1.5f);
        preSetLocations[2].locHeading = -132.0f;

        // Position W 
        preSetLocations[3].locPos = new Vector3(-2.09f, 0.6f, 1.22f);
        preSetLocations[3].locHeading = 133.0f;

        // Position 1 max distance infron of TV1
        preSetLocations[1].locPos = new Vector3(0.145f, 0.6f, 2.42190f);
        preSetLocations[1].locHeading = -179.0f;

        //Position R at the center of the Arena
        preSetLocations[0].locPos = new Vector3(0.0f, 0.6f, 0.0f);
        preSetLocations[0].locHeading = 180.0f;


        //Position T viewing TV1 and TV2
        preSetLocations[4].locPos = new Vector3(-2.654f, 0.6f, 0.246f);
        preSetLocations[4].locHeading = 112.5f;

        //Position Y at the center viewing the Wall between TV1 and TV2
        preSetLocations[5].locPos = new Vector3(0.01f, 0.6f, 0.01f);
        preSetLocations[5].locHeading = 120.0f;

        //Position A at the corner viewing the Wall between TV2 and empty wall (TV2 on the left)
        preSetLocations[6].locPos = new Vector3(2.4f, 0.6f, 0.06f);
        preSetLocations[6].locHeading = 90.0f;

        //Position S infrontof  the Wall between TV1 and TV2
        preSetLocations[7].locPos = new Vector3(2.04f, 0.6f, -1.25f);
        preSetLocations[7].locHeading = 120.0f;

        //Position D at the center viewing the Wall between TV1 and empty wall (TV1 on the right)
        preSetLocations[8].locPos = new Vector3(1.39f, 0.6f, -2.42f);
        preSetLocations[8].locHeading = 155.0f;

        //Position F infront of TV1 into the active area
        preSetLocations[9].locPos = new Vector3(-0.09f, 0.6f, -2.23f);
        preSetLocations[9].locHeading = 179.9f;

        // Position 2 max distance infron of TV2
        preSetLocations[10].locPos = new Vector3(-2.00f, 0.6f, -1.29f);
        preSetLocations[10].locHeading = 60.0f;

        //Position 3 infront of TV3 into the active area
        preSetLocations[11].locPos = new Vector3(2.1f, 0.6f, -1.16f);
        preSetLocations[11].locHeading = -60.0f;



        // ATTN:  ArenaBackMoveDuration is in ms
        durationBackMoveToGetReward = PlayerPrefs.GetFloat("ArenaBackMoveDuration", 300.0f)/1000.0f;
        durationRestInterval = PlayerPrefs.GetFloat("ArenaBackMoveRestInterval", 1200.0f) / 1000.0f;





    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        //tomkil movementMultiplier = 12.0f / TravelTime;
        movementMultiplier = distanceToTravel / TravelTime;
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
        Quaternion deltaRotation;
        
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

                if (ArenaGame.theArenaGame.alterRotationalSpeed==true)
                {
                    RotSpeed = RotSpeed + ArenaGame.theArenaGame.alterationDirection * stepRotSpeed;
                    if (RotSpeed > maxRotSpeed)
                        RotSpeed = maxRotSpeed;
                    else if (RotSpeed < minRotSpeed)
                        RotSpeed = minRotSpeed;
                    ArenaGame.theArenaGame.alterRotationalSpeed = false;
                }


                if (ArenaGame.theArenaGame.alterLinearSpeed == true)
                {
                    TravelTime = TravelTime + ArenaGame.theArenaGame.alterationDirection * stepLinSpeed;
                    if (TravelTime > maxTimeTravel)
                        TravelTime = maxTimeTravel;
                    else if (TravelTime < minTimeTravel)
                        TravelTime = minTimeTravel;

                    movementMultiplier = distanceToTravel / TravelTime;

                    ArenaGame.theArenaGame.alterLinearSpeed = false;
                }


                if (ArenaGame.theArenaGame.doRotation==true)
                {

                    currentRot = ArenaGame.theArenaGame.rotationDirection * RotSpeed;

                    
                    deltaRotation = Quaternion.Euler(0f, currentRot * Time.fixedDeltaTime, 0f);
                    rb.MoveRotation(rb.rotation * deltaRotation);
                    
                    ArenaGame.theArenaGame.doRotation = false;
                    return;
                }

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

                /*
                if (moveX < 0.0f)
                    moveX = 1.0f;
                */

                

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
                        //rev moveX = 1.0f; 
                        moveX -= 1.0f;
                    }
                    else if (prevX > 0.0f)
                    {
                        moveX += 1.0f;
                    }
                }

                // rev moveX *= -1;

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
            //rev effective_Y = -moveX;
        }

        if (PlayerPrefs.GetInt("ArenaEnableBackMoveRew", 0) == 1)
        {
            timeNow = Time.time;

            if (inRestInterval != true)
            {


                if (inBackwardMode == true)
                {
                    if (moveX < 0.0f)
                    {
                        // check duration
                        if (timeNow - startTimeMovingBack > durationBackMoveToGetReward)
                        {
                            serialArena.GiveJuice();
                            Debug.Log("[" + timeNow.ToString("F3") + "] BACKWARD Move Reward Given!");
                            inBackwardMode = false;
                            inRestInterval = true;
                            startTimeRestInterval = timeNow;
                        }
                    }
                    else
                    {
                        //reset flag
                        inBackwardMode = false;
                    }
                }
                else
                {
                    if (moveX < 0.0f && inRestInterval == false)
                    {
                        inBackwardMode = true;
                        startTimeMovingBack = Time.time;

                    }
                }
            }
            else
            {
                if (timeNow - startTimeRestInterval > durationRestInterval)
                    inRestInterval = false;
            }
            


        }
        //moveDirection = orientation.forward * moveY + orientation.right * moveX;

        moveDirection = orientation.forward * effective_Y + orientation.right * effective_X;


        rb.AddForce(moveDirection.normalized * movementMultiplier * 0.2f, ForceMode.Force);
        /*
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
        */

        //currentSpeed = moveY * movementMultiplier;
        //currentRot = moveX * RotSpeed;

        currentSpeed = effective_Y * movementMultiplier;
        currentRot = effective_X * RotSpeed;

        rb.MovePosition(transform.position + transform.forward * currentSpeed * Time.deltaTime);
        deltaRotation = Quaternion.Euler(0f, currentRot * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
