using Foraging3Screens;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using ViveSR.anipal.Eye;
using static PlayerMovement;
using static SerialArena;

public class ArenaGame : MonoBehaviour
{
    public static ArenaGame theArenaGame;
    public GameObject TV1;
    public GameObject TV2;
    public GameObject TV3;
    public GameObject player;

    public uint updates;
    public string timestmp;
    public float time_in_w, fps;
    public float JOYPress;

    public int ArenaJoysticMode;

    public bool resetPostion = false;
    public int presetPositionNumber = 0;

    public bool doRotation = false;
    public float rotationDirection;

    public bool alterRotationalSpeed = false;
    public bool alterLinearSpeed = false;
    public float alterationDirection;

    private GameObject playerObj = null;

    private GameObject boxTV1Obj = null;
    private GameObject boxTV2Obj = null;
    private GameObject boxTV3Obj = null;
    private GameObject Screen1Obj = null;

    private GameObject theScoreDisp = null;
    private TextMeshPro theScoreTMP;

    public TVcontrol boxTVCntrl1 = null;
    public TVcontrol boxTVCntrl2 = null;
    public TVcontrol boxTVCntrl3 = null;

    private GameObject colScreenBox1 = null;
    private GameObject colScreenBox2 = null;
    private GameObject colScreenBox3 = null;


    private Transform trans, childTrans;
    private SpriteRenderer aSpriteRenderer;
    private int col_state;

    private int keyEscapePressed;
    float timeToEndTrial;
    float timeToResetTrainingMode2;
    int trainigMode2resetDuration;

    static public FG3Screens fg3Sc;

    static public int theScore = 0; // Score Keeping


    List<float> log_timeStamp = new List<float>();

    List<float> log_x_PlayerPos = new List<float>();
    List<float> log_y_PlayerPos = new List<float>();
    List<float> log_z_PlayerPos = new List<float>();

    List<float> log_x_PlayerAngle = new List<float>();
    List<float> log_y_PlayerAngle = new List<float>();
    List<float> log_z_PlayerAngle = new List<float>();

    List<float> log_JoystickButton = new List<float>();

    // Box specific data
    List<float> log_box1timeBtnPressd = new List<float>();
    List<byte> log_box1RewardGiven = new List<byte>();
    List<float> log_box1timeRewardGiven = new List<float>();

    List<float> log_box1timeToReward = new List<float>();

    List<float> log_box2timeBtnPressd = new List<float>();
    List<byte> log_box2RewardGiven = new List<byte>();
    List<float> log_box2timeRewardGiven = new List<float>();

    List<float> log_box2timeToReward = new List<float>();

    List<float> log_box3timeBtnPressd = new List<float>();
    List<byte> log_box3RewardGiven = new List<byte>();
    List<float> log_box3timeRewardGiven = new List<float>();

    List<float> log_box3timeToReward = new List<float>();

    List<float> log_comb_eye_data_x_dir = new List<float>();
    List<float> log_comb_eye_data_y_dir = new List<float>();
    List<float> log_comb_eye_data_z_dir = new List<float>();
    List<ulong> log_comb_eye_data_validity_bitmask = new List<ulong>();


    List<float> log_left_eye_data_x_dir = new List<float>();
    List<float> log_left_eye_data_y_dir = new List<float>();
    List<float> log_left_eye_data_z_dir = new List<float>();
    List<float> log_left_eye_ppl_diam = new List<float>();
    List<float> log_left_eye_openness = new List<float>();

    List<ulong> log_left_eye_data_validity_bitmask = new List<ulong>();



    List<float> log_right_eye_data_x_dir = new List<float>();
    List<float> log_right_eye_data_y_dir = new List<float>();
    List<float> log_right_eye_data_z_dir = new List<float>();
    List<float> log_right_eye_ppl_diam = new List<float>();
    List<float> log_right_eye_openness = new List<float>();

    List<ulong> log_right_eye_data_validity_bitmask = new List<ulong>();



    List<byte> log_calibration_status = new List<byte>();




    //
    GameObject theCalibrationCntrl;
    GameObject theGazeControler;
    GameObject theGazeVisualizer;





    // Eye tracker data

    public EyeData data = new EyeData();

    static public bool VR_HD_Calibrated = false;
    static public bool LOG_EYE_TRACKER = true;

    //
    static public bool COLOR_SCREENS_ON_BOXES = true;

    static bool DATA_SAVED = false;

    public bool ARENA_INITIALIZED = false;

    public enum TrainingMode
    { None, ScreenFront, ScreenStraight }

    public TrainingMode ArenaTrainigMode = TrainingMode.None;


    // This script acts as the Main Manager tk
    private void Awake()
    {
        theArenaGame = this;

        // Check if got here as 1st run or through quit and restart by menu
        if (fg3Sc == null)
        {
            fg3Sc = new FG3Screens();
            //init component
            fg3Sc.screen();
            fg3Sc.setup();
            fg3Sc.clut();
        }
        timeToEndTrial = Time.time + (float)PlayerPrefs.GetFloat("TrialDuration");

        if (PlayerPrefs.GetInt("ArenaMode") != 1)
        {
            var error = SRanipal_Eye_Framework.Status;
            Debug.Log("SRanipal Engine Status: " + error.ToString());
            //Tomkil calib ok
            //int result = ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(System.IntPtr.Zero);
            //Debug.Log("SRanipal Engine Calib: " + result.ToString());

            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
            {
                SRanipal_Eye_Framework.Instance.StartFramework();
            }
            //tomkl check also cases not support error etc

            error = SRanipal_Eye_Framework.Status;
            Debug.Log("SRanipal Engine Status: " + error.ToString());

        }


        // Check if color boxes need to color marked
        // No need directly set to COLOR_SCREENS_ON_BOXES
        //if (PlayerPrefs.GetString("ColorOnBoxes") == "1")
        //    COLOR_SCREENS_ON_BOXES = true;

        //Get the Calibration Controller

        theGazeControler = GameObject.Find("Gaze Controller");
        if (theGazeControler == null)
            Debug.Log("Gaze Controller not found!");
        else
        {
            theGazeVisualizer = GameObject.Find("Gaze Visualizer");
            if (theGazeVisualizer == null)
                Debug.Log("Gaze Visualizer not found!");
            else
                theGazeVisualizer.SetActive(false);
        }


        theCalibrationCntrl = GameObject.Find("Calibration Controller");
        if (theCalibrationCntrl == null)
            Debug.Log("Calibration Control not found!");
        else
        {
            // Disable the Calibration Controler

            theCalibrationCntrl.SetActive(false);
        }


        ArenaJoysticMode = PlayerPrefs.GetInt("ArenaJoystickMode", 0);


    }

    // Start is called before the first frame update

    void Start()
    {


        updates = 0;
        if (playerObj == null)
            playerObj = GameObject.Find("Player");

        if (boxTV1Obj == null)
        {
            boxTV1Obj = GameObject.Find("BoxTV1");
            boxTVCntrl1 = boxTV1Obj.GetComponentInChildren<TVcontrol>();
        }

        if (boxTV2Obj == null)
        {
            boxTV2Obj = GameObject.Find("BoxTV2");
            boxTVCntrl2 = boxTV2Obj.GetComponentInChildren<TVcontrol>();
        }

        if (boxTV3Obj == null)
        {
            boxTV3Obj = GameObject.Find("BoxTV3");
            boxTVCntrl3 = boxTV3Obj.GetComponentInChildren<TVcontrol>();
        }

        // Get the color screens

        if (colScreenBox1 == null)
        {
            colScreenBox1 = GameObject.Find("TVcurtain1");

        }
        if (colScreenBox2 == null)
        {
            colScreenBox2 = GameObject.Find("TVcurtain2");

        }
        if (colScreenBox3 == null)
        {
            colScreenBox3 = GameObject.Find("TVcurtain3");

        }

        colScreenBox1.SetActive(COLOR_SCREENS_ON_BOXES);
        colScreenBox2.SetActive(COLOR_SCREENS_ON_BOXES);
        colScreenBox3.SetActive(false);


        Renderer aRend;
        if (COLOR_SCREENS_ON_BOXES == true)
        {

            //Set the colors of TVcutrtains
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //
            aRend = colScreenBox1.GetComponent<Renderer>();

            aRend.material.color = new UnityEngine.Color32(12, 12, 12, 255);
            //aRend.material.color = new UnityEngine.Color32(243, 248, 18, 255); //ATTN yellowish interference with yellow activation frame on TVs
            //aRend.material.color = new UnityEngine.Color32(156,124,0,255);
            //aRend.material.color = new UnityEngine.Color(255, 0, 0, 255);

            aRend = colScreenBox2.GetComponent<Renderer>();
            aRend.material.color = new UnityEngine.Color32(250, 250, 250, 255);
            //aRend.material.color = new UnityEngine.Color32(12, 96, 10, 255);
            //aRend.material.color = new UnityEngine.Color(0, 255, 0, 255);

            aRend = colScreenBox3.GetComponent<Renderer>();
            aRend.material.color = new UnityEngine.Color32(230, 195, 231, 255);
            //aRend.material.color = new UnityEngine.Color(0, 0, 255, 255);

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            trainigMode2resetDuration = PlayerPrefs.GetInt("ArenaTrainigMode2ResetTime", 90);
            timeToResetTrainingMode2 = Time.time + trainigMode2resetDuration;
            ARENA_INITIALIZED = true;
        }




        //get obj transform
        trans = boxTV1Obj.transform;
        childTrans = trans.Find("Screen");
        Screen1Obj = childTrans.gameObject;

        aSpriteRenderer = Screen1Obj.GetComponent<SpriteRenderer>();

        //get the score keeping object
        theScoreDisp = GameObject.Find("Score");
        theScoreTMP = theScoreDisp.GetComponent<TextMeshPro>();

        theScore = 0;

        // Disable display of score
        theScoreDisp.SetActive(false);

        //Reset Latch Data save latch
        DATA_SAVED = false;

        int trainingMode = PlayerPrefs.GetInt("ArenaTrainingMode");

        switch (trainingMode)
        {
            case 0:
                ArenaTrainigMode = TrainingMode.None;
                break;
            case 1:
                ArenaTrainigMode = TrainingMode.ScreenFront;
                break;
            case 2:
                ArenaTrainigMode = TrainingMode.ScreenStraight;
                break;
            default:
                ArenaTrainigMode = TrainingMode.None;
                Debug.Log("ArenaGame: Unknown training mode. Setting training mode to: None");
                break;


        }


    }

    // Update is called once per frame
    void Update()
    {

        if (PlayerPrefs.GetInt("ArenaMode") == 1)
        {

        }
        else
        {
            SaveDataToMem();
        }
        //Cause FixedUpdates execs 50 persec
        //if (updates >= 3)
        //{

        //  updates = 0;
        //Log all the trial vars into coresponding lists
        //
        //

        //   SaveDataToMem();
        //   SaveData();

        //this.updates += 1;
        //print(sharedmovement.moveX);
        
        //Handle keystrokes
        if (Input.GetKey(KeyCode.Escape))
        {
            keyEscapePressed = 1;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && keyEscapePressed == 1)
        {
            if (DATA_SAVED == false)
            {
                if (PlayerPrefs.GetInt("ArenaMode") == 1)
                {

                }
                else
                {
                    SaveData();
                    SaveDataToMem();
                    SaveDataToFile();
                }

                DATA_SAVED = true;
                keyEscapePressed = 0;
                // Display Score and wait
                //theScoreTMP.text = theScore.ToString();
                //theScoreDisp.SetActive(true);

                // StartCoroutine will change scene which triggers saving in ArenaMode==1 !!!
                StartCoroutine(ShowScore());

                //SceneManager.LoadScene("MainMenu");

            }
        }
        else if (Input.GetKey("["))
        {
            keyEscapePressed = 0;
            if (PlayerPrefs.GetInt("ArenaMode") == 1)
            {

            }
            else
            {

                SaveData();
                SaveDataToMem();
                SaveDataToFile();
            }

        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            resetPostion = true;
            presetPositionNumber = 0;
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            resetPostion = true;
            presetPositionNumber = 2;
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            resetPostion = true;
            presetPositionNumber = 3;
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            resetPostion = true;
            presetPositionNumber = 1;
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            resetPostion = true;
            presetPositionNumber = 4;
        }
        else if (Input.GetKeyUp(KeyCode.Y))
        {
            resetPostion = true;
            presetPositionNumber = 5;
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            resetPostion = true;
            presetPositionNumber = 6;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            resetPostion = true;
            presetPositionNumber = 7;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            resetPostion = true;
            presetPositionNumber = 8;
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            resetPostion = true;
            presetPositionNumber = 9;
        }
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            rotationDirection = -1.0f;
            doRotation = true;
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            rotationDirection = 1.0f;
            doRotation = true;
        }
        else if (Input.GetKeyUp(KeyCode.Comma))
        {
            alterRotationalSpeed = true;
            alterationDirection = -1.0f;

        }
        else if (Input.GetKeyUp(KeyCode.Period))
        {
            alterRotationalSpeed = true;
            alterationDirection = 1.0f;

        }
        else if (Input.GetKeyUp(KeyCode.LeftBracket))
        {
            alterLinearSpeed = true;
            alterationDirection = -1.0f;

        }
        else if (Input.GetKeyUp(KeyCode.RightBracket))
        {
            alterLinearSpeed = true;
            alterationDirection = 1.0f;

        }

        else if (Input.GetKeyUp(KeyCode.Space))
        {
            //Give Reward;
            serialArena.GiveJuice();
            Debug.Log("[" + Time.time.ToString("F3") + "] MANUALLY given reward");

        }



    }

    public void FixedUpdate()
    {

        

        if (Time.time > timeToEndTrial)
        {




            if (DATA_SAVED == false)
            {

                if (PlayerPrefs.GetInt("ArenaMode") == 1)
                {

                }
                else
                {
                    SaveData();
                    SaveDataToMem();
                    SaveDataToFile();

                }

                keyEscapePressed = 0;


                DATA_SAVED = true;
                // Display Score and wait
                //theScoreTMP.text = theScore.ToString();
                //theScoreDisp.SetActive(true);


                StartCoroutine(ShowScore());

                //SceneManager.LoadScene("MainMenu");

            }
        }
        else if (Time.time > timeToResetTrainingMode2)
        {

            timeToResetTrainingMode2 = Time.time + trainigMode2resetDuration;
            //resetPostionTrainingMode2 = true;
        }

    }
    public void SaveDataToMem()
    {
        float rot;

        ViveSR.Error error = SRanipal_Eye_API.GetEyeData(ref data);
        float x, lx, rx;
        float y, ly, ry;
        float z, lz, rz;
        ulong l_eye_data_validity_mask, r_eye_data_validity_mask, cmb_eye_data_validity_mask;

        var left = new SingleEyeData();
        var right = new SingleEyeData();
        var combined = new CombinedEyeData();

        if (error == ViveSR.Error.WORK)
        {
            left = data.verbose_data.left;
            right = data.verbose_data.right;
            combined = data.verbose_data.combined;

            x = combined.eye_data.gaze_direction_normalized.x;
            y = combined.eye_data.gaze_direction_normalized.y;
            z = combined.eye_data.gaze_direction_normalized.z;

            cmb_eye_data_validity_mask = combined.eye_data.eye_data_validata_bit_mask;

            lx = left.gaze_direction_normalized.x;
            ly = left.gaze_direction_normalized.y;
            lz = left.gaze_direction_normalized.z;

            rx = right.gaze_direction_normalized.x;
            ry = right.gaze_direction_normalized.y;
            rz = right.gaze_direction_normalized.z;

            l_eye_data_validity_mask = left.eye_data_validata_bit_mask;
            r_eye_data_validity_mask = right.eye_data_validata_bit_mask;


        }
        else
        {
            Debug.Log("VIVE Status: " + error.ToString());
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;

            lx = 0.0f;
            ly = 0.0f;
            lz = 0.0f;

            rx = 0.0f;
            ry = 0.0f;
            rz = 0.0f;


            left.pupil_diameter_mm = 0.0f;
            left.eye_openness = 0.0f;
            right.pupil_diameter_mm = 0.0f;
            right.eye_openness = 0.0f;

            cmb_eye_data_validity_mask = 0;
            l_eye_data_validity_mask = 0;
            r_eye_data_validity_mask = 0;



        }
        //

        // Player Position
        log_timeStamp.Add(Time.time);
        log_x_PlayerPos.Add(playerObj.transform.position.x);
        log_y_PlayerPos.Add(playerObj.transform.position.y);
        log_z_PlayerPos.Add(playerObj.transform.position.z);

        rot = playerObj.transform.rotation.eulerAngles.y;
        if (rot > 180f)
            rot -= 360f;

        log_x_PlayerAngle.Add(playerObj.transform.localRotation.x);
        log_y_PlayerAngle.Add(rot);
        log_z_PlayerAngle.Add(playerObj.transform.localRotation.z);

        log_JoystickButton.Add(sharedmovement.press);
        //
        //
        log_box1timeBtnPressd.Add(boxTVCntrl1.timePointButtonPressed);
        if (boxTVCntrl1.RewardLatch == TVcontrol.RewardState.Given)
        {
            log_box1RewardGiven.Add((byte)1);
            log_box1timeRewardGiven.Add(boxTVCntrl1.timePointButtonPressed);
            log_box1timeToReward.Add(boxTVCntrl1.timeToReward);

            //            ResetLatch

            boxTVCntrl1.RewardLatch = TVcontrol.RewardState.NotGiven;

        }
        else
        {
            log_box1RewardGiven.Add((byte)0);
            log_box1timeRewardGiven.Add(boxTVCntrl1.timePointButtonPressed);
            log_box1timeToReward.Add(boxTVCntrl1.timeToReward);

        }

        log_box2timeBtnPressd.Add(boxTVCntrl2.timePointButtonPressed);
        if (boxTVCntrl2.RewardLatch == TVcontrol.RewardState.Given)
        {
            log_box2RewardGiven.Add((byte)1);
            log_box2timeRewardGiven.Add(boxTVCntrl2.timePointButtonPressed);
            log_box2timeToReward.Add(boxTVCntrl2.timeToReward);

            //            ResetLatch

            boxTVCntrl2.RewardLatch = TVcontrol.RewardState.NotGiven;

        }
        else
        {
            log_box2RewardGiven.Add((byte)0);
            log_box2timeRewardGiven.Add(boxTVCntrl2.timePointButtonPressed);
            log_box2timeToReward.Add(boxTVCntrl2.timeToReward);

        }

        log_box3timeBtnPressd.Add(boxTVCntrl3.timePointButtonPressed);
        if (boxTVCntrl3.RewardLatch == TVcontrol.RewardState.Given)
        {
            log_box3RewardGiven.Add((byte)1);
            log_box3timeRewardGiven.Add(boxTVCntrl3.timePointButtonPressed);
            log_box3timeToReward.Add(boxTVCntrl3.timeToReward);

            //            ResetLatch

            boxTVCntrl3.RewardLatch = TVcontrol.RewardState.NotGiven;

        }
        else
        {
            log_box3RewardGiven.Add((byte)0);
            log_box3timeRewardGiven.Add(boxTVCntrl3.timePointButtonPressed);
            log_box3timeToReward.Add(boxTVCntrl3.timeToReward);

        }

        if (LOG_EYE_TRACKER)
        {

            log_comb_eye_data_x_dir.Add(x);
            log_comb_eye_data_y_dir.Add(y);
            log_comb_eye_data_z_dir.Add(z);
            log_comb_eye_data_validity_bitmask.Add(cmb_eye_data_validity_mask);



            log_left_eye_data_x_dir.Add(lx);
            log_left_eye_data_y_dir.Add(ly);
            log_left_eye_data_z_dir.Add(lz);

            log_left_eye_ppl_diam.Add(left.pupil_diameter_mm);
            log_left_eye_openness.Add(left.eye_openness);

            log_left_eye_data_validity_bitmask.Add(r_eye_data_validity_mask);

            log_right_eye_data_x_dir.Add(rx);
            log_right_eye_data_y_dir.Add(ry);
            log_right_eye_data_z_dir.Add(rz);

            log_right_eye_ppl_diam.Add(right.pupil_diameter_mm);
            log_right_eye_openness.Add(right.eye_openness);

            log_right_eye_data_validity_bitmask.Add(l_eye_data_validity_mask);



            log_calibration_status.Add(VR_HD_Calibrated ? (byte)1 : (byte)0);
        }


    }
    public void SaveData()
    {
        float rot;

        time_in_w = Time.time;
        timestmp = time_in_w.ToString("f6");
        fps = Time.frameCount / time_in_w;

        JOYPress = sharedmovement.press;
        Debug.Log("Saving Data. Updates so far: " + timestmp);
        Debug.Log("Achieved FPS:" + fps.ToString("f3"));
        Debug.Log("Player Pos ( x, y, z ) = ( " + playerObj.transform.position.x + ", " + playerObj.transform.position.y + ", " + playerObj.transform.position.z + " )");

        rot = playerObj.transform.rotation.eulerAngles.y;
        if (rot > 180f)
            rot -= 360f;

        Debug.Log("Player Rot ( x, y, z ) = ( " + playerObj.transform.localRotation.x + ", " + rot + ", " + playerObj.transform.localRotation.z + " )");
        Debug.Log("Joystick button state:" + JOYPress.ToString("f4"));

        if (col_state == 0)
        {
            //aSpriteRenderer.color = Color.red;



            col_state = 1;
        }
        else
        {
            //aSpriteRenderer.color = Color.blue;
            col_state = 0;
        }

        Debug.Log("Sprite" + aSpriteRenderer.sprite);





    }
    void SaveDataToFile()
    {
        //Get path
        string path, pathname_cont, pathname_param, recordLine, subjName;


        string Header, formatStr;

        subjName = PlayerPrefs.GetString("SubjectName");

        if (subjName.Length == 0)
            subjName = "Subject0_";
        else
            subjName += "_";

        path = PlayerPrefs.GetString("DataAndConfigPath");
        //string almostUniqueFileName = $@"{DateTime.Now.Ticks}"; // maybe put a date om front plus ticks?
        //pathname_cont = path + "\\" + subjName + "FMA_" + almostUniqueFileName+ "_cont.txt";
        //pathname_param = path + "\\" + subjName + "FMA_" + almostUniqueFileName + "_param.txt";
        pathname_cont = path + "\\" + subjName + "_cont.txt";
        pathname_param = path + "\\" + subjName + "_param.txt";




        if (LOG_EYE_TRACKER)
        {
            Header = "time_stamp,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z," +
                "b1_press,b1_rew,b1_time,b1_time_next_rew,b2_press,b2_rew,b2_time,b2_time_next_rew,b3_press,b3_rew,b3_time,b3_time_next_rew," +
                "cmb_eye_dir_x,cmb_eye_dir_y,cmb_eye_dir_z,cmb_eye_valid," +
                "l_eye_dir_x,l_eye_dir_y,l_eye_dir_z,l_eye_dm,l_eye_op,l_eye_valid," +
                "r_eye_dir_x,r_eye_dir_y,r_eye_dir_z,r_eye_dm,r_eye_op,r_eye_valid," +
                "calibration\n";
            formatStr = "{0},{1},{2},{3},{4},{5},{6}," +
                "{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}," +
                "{19},{20},{21},{22}," +
                "{23},{24},{25},{26},{27},{28}," +
                "{29},{30},{31},{32},{33},{34}," +
                "{35}\n";

            File.AppendAllText(pathname_cont, Header);

            for (int i = 0; i < log_timeStamp.Count; i++)
            {
                recordLine = string.Format(formatStr,
                    log_timeStamp[i].ToString("F6"),
                    log_x_PlayerPos[i].ToString("F6"),
                    log_y_PlayerPos[i].ToString("F6"),
                    log_z_PlayerPos[i].ToString("F6"),
                    log_x_PlayerAngle[i].ToString("F3"),
                    log_y_PlayerAngle[i].ToString("F3"),
                    log_z_PlayerAngle[i].ToString("F3"),
                    log_box1timeBtnPressd[i].ToString("F3"),
                    log_box1RewardGiven[i].ToString(),
                    log_box1timeRewardGiven[i].ToString("F6"),
                    log_box1timeToReward[i].ToString("F6"),
                    log_box2timeBtnPressd[i].ToString("F3"),
                    log_box2RewardGiven[i].ToString(),
                    log_box2timeRewardGiven[i].ToString("F6"),
                    log_box2timeToReward[i].ToString("F6"),
                    log_box3timeBtnPressd[i].ToString("F3"),
                    log_box3RewardGiven[i].ToString(),
                    log_box3timeRewardGiven[i].ToString("F6"),
                    log_box3timeToReward[i].ToString("F6"),
                    log_comb_eye_data_x_dir[i].ToString("f3"),
                    log_comb_eye_data_y_dir[i].ToString("f3"),
                    log_comb_eye_data_z_dir[i].ToString("f3"),
                    log_comb_eye_data_validity_bitmask[i].ToString(),
                    log_left_eye_data_x_dir[i].ToString("f3"),
                    log_left_eye_data_y_dir[i].ToString("f3"),
                    log_left_eye_data_z_dir[i].ToString("f3"),
                    log_left_eye_ppl_diam[i].ToString("f4"),
                    log_left_eye_openness[i].ToString("f4"),
                    log_left_eye_data_validity_bitmask[i].ToString(),
                    log_right_eye_data_x_dir[i].ToString("f3"),
                    log_right_eye_data_y_dir[i].ToString("f3"),
                    log_right_eye_data_z_dir[i].ToString("f3"),
                    log_right_eye_ppl_diam[i].ToString("f4"),
                    log_right_eye_openness[i].ToString("f4"),
                    log_right_eye_data_validity_bitmask[i].ToString(),
                    log_calibration_status[i].ToString("f4")

                    ); ;
                File.AppendAllText(pathname_cont, recordLine);
            }
        }
        else
        {
            Header = "time_stamp,pos_x,pos_y,pos_z,rot_x,rot_y,rot_z," +
                "b1_press,b1_rew,b1_time,b1_time_next_rew,b2_press,b2_rew,b2_time,b2_time_next_rew,b3_press,b3_rew,b3_time,b3_time_next_rew";

            formatStr = "{0},{1},{2},{3},{4},{5},{6}," +
                "{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}\n";

            File.AppendAllText(pathname_cont, Header);

            for (int i = 0; i < log_timeStamp.Count; i++)
            {
                recordLine = string.Format(formatStr,
                    log_timeStamp[i].ToString("F6"),
                    log_x_PlayerPos[i].ToString("F6"),
                    log_y_PlayerPos[i].ToString("F6"),
                    log_z_PlayerPos[i].ToString("F6"),
                    log_x_PlayerAngle[i].ToString("F3"),
                    log_y_PlayerAngle[i].ToString("F3"),
                    log_z_PlayerAngle[i].ToString("F3"),
                    log_box1timeBtnPressd[i].ToString("F3"),
                    log_box1RewardGiven[i].ToString(),
                    log_box1timeRewardGiven[i].ToString("F6"),
                    log_box1timeToReward[i].ToString("F6"),
                    log_box2timeBtnPressd[i].ToString("F3"),
                    log_box2RewardGiven[i].ToString(),
                    log_box2timeRewardGiven[i].ToString("F6"),
                    log_box2timeToReward[i].ToString("F6"),
                    log_box1timeBtnPressd[i].ToString("F3"),
                    log_box3RewardGiven[i].ToString(),
                    log_box3timeRewardGiven[i].ToString("F6"),
                    log_box3timeToReward[i].ToString("F6")

                    );
                File.AppendAllText(pathname_cont, recordLine);
            }
        }


        Debug.Log("Log file cont" + pathname_cont + " saved.\n");

        //

        Header = "CorrectPush,WrongPush,RewardRadius,BoxMean1,BoxMean2,BoxMean3,BoxNoise1,BoxNoise2,BoxNoise3,TrialDuration,DataAndCofigPath,LogEyeTracker,ColoredBoxes,MovementMultiplier\n";

        formatStr = "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}\n";

        File.AppendAllText(pathname_param, Header);
        recordLine = string.Format(formatStr,
            PlayerPrefs.GetFloat("CorrectPush").ToString(),
            PlayerPrefs.GetFloat("WrongPush").ToString(),
            PlayerPrefs.GetFloat("RewardRadius").ToString(),
            PlayerPrefs.GetFloat("BoxMean1").ToString(),
            PlayerPrefs.GetFloat("BoxMean2").ToString(),
            PlayerPrefs.GetFloat("BoxMean3").ToString(),
            PlayerPrefs.GetFloat("BoxNoise1").ToString(),
            PlayerPrefs.GetFloat("BoxNoise2").ToString(),
            PlayerPrefs.GetFloat("BoxNoise3").ToString(),
            PlayerPrefs.GetFloat("TrialDuration").ToString(),
            LOG_EYE_TRACKER == true ? "1" : "0",
            COLOR_SCREENS_ON_BOXES == true ? "1" : "0",
            PlayerPrefs.GetFloat("MovementMultiplier").ToString()

             );
        File.AppendAllText(pathname_param, recordLine);

    }

    IEnumerator ShowScore()
    {
        theScoreTMP.text = theScore.ToString();
        theScoreDisp.SetActive(true);
        //Debug.Log("CorStart: "+Time.time);
        yield return new WaitForSecondsRealtime(6f);
        //Debug.Log("CorStop : "+Time.time);
#if UNITY_EDITOR
        EditorSceneManager.LoadScene("MainMenu");
#else
        SceneManager.LoadScene("MainMenu");
#endif
    }

}
